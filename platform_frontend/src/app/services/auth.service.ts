import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError, firstValueFrom } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// Google Identity Services types
declare global {
  interface Window {
    google: any;
  }
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface UserInfo {
  id: number;
  email: string;
  displayName: string;
  photoUrl?: string;
  emailVerified: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number; // in seconds
  user: UserInfo;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = environment.apiUrl + '/auth';
  private readonly googleClientId = environment.googleClientId;
  
  private userSubject = new BehaviorSubject<UserInfo | null>(null);
  public user$ = this.userSubject.asObservable();
  
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private tokenExpiryTime: number | null = null;
  private isSigningIn = false;

  constructor(private http: HttpClient) {
    this.initializeAuth();
    this.loadGoogleIdentityServices();
  }

  /**
   * Sign in with Google using Google Identity Services
   */
  async signInWithGoogle(): Promise<AuthResponse> {
    if (this.isSigningIn) {
      throw new Error('Sign-in already in progress');
    }

    this.isSigningIn = true;

    return new Promise((resolve, reject) => {
      if (!window.google) {
        this.isSigningIn = false;
        reject(new Error('Google Identity Services not loaded'));
        return;
      }

      // Initialize Google Sign-In
      window.google.accounts.id.initialize({
        client_id: this.googleClientId,
        callback: async (response: any) => {
          try {
            const authResponse = await this.authenticateWithBackend(response.credential);
            this.isSigningIn = false;
            resolve(authResponse);
          } catch (error) {
            this.isSigningIn = false;
            reject(error);
          }
        },
        auto_select: false,
        cancel_on_tap_outside: true
      });

      // Prompt for sign-in
      window.google.accounts.id.prompt((notification: any) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
          // Fallback to popup
          this.signInWithPopup()
            .then(result => {
              this.isSigningIn = false;
              resolve(result);
            })
            .catch(error => {
              this.isSigningIn = false;
              reject(error);
            });
        }
      });
    });
  }

  /**
   * Sign in with Google using popup (fallback method)
   */
  private async signInWithPopup(): Promise<AuthResponse> {
    return new Promise((resolve, reject) => {
      if (!window.google) {
        reject(new Error('Google Identity Services not loaded'));
        return;
      }

      const client = window.google.accounts.oauth2.initTokenClient({
        client_id: this.googleClientId,
        scope: 'openid email profile',
        callback: async (response: any) => {
          if (response.error) {
            reject(new Error(response.error));
            return;
          }

          try {
            // Get ID token from access token
            const idToken = await this.getIdTokenFromAccessToken(response.access_token);
            const authResponse = await this.authenticateWithBackend(idToken);
            resolve(authResponse);
          } catch (error) {
            reject(error);
          }
        }
      });

      client.requestAccessToken();
    });
  }

  /**
   * Authenticate with backend using Google ID token
   */
  private async authenticateWithBackend(idToken: string): Promise<AuthResponse> {
    const request: GoogleLoginRequest = { idToken };
    
    const httpRequest = this.http.post<AuthResponse>(`${this.apiUrl}/google`, request)
      .pipe(
        tap(response => this.setSession(response)),
        catchError(error => {
          console.error('Authentication failed:', error);
          return throwError(() => new Error('Authentication failed'));
        })
      );
    
    return firstValueFrom(httpRequest);
  }

  /**
   * Get ID token from access token (for popup flow)
   */
  private async getIdTokenFromAccessToken(accessToken: string): Promise<string> {
    try {
      // Use Google's userinfo endpoint to get user information
      const response = await fetch(`https://www.googleapis.com/oauth2/v2/userinfo?access_token=${accessToken}`);
      const userInfo = await response.json();
      
      if (userInfo.error) {
        throw new Error('Invalid access token');
      }

      // For the popup flow, we'll create a simple JWT-like token with user info
      // In a real implementation, you'd want to get the actual ID token from Google
      // For now, we'll use the access token as a fallback
      // The backend should be able to validate this with Google's API
      return accessToken;
    } catch (error) {
      console.error('Failed to get user info from access token:', error);
      throw new Error('Failed to validate access token');
    }
  }

  /**
   * Refresh access token using refresh token
   */
  async refresh(): Promise<AuthResponse | null> {
    if (!this.refreshToken) {
      return null;
    }

    try {
      const request: RefreshTokenRequest = { refreshToken: this.refreshToken };
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, request)
      );
      
      if (response) {
        this.setSession(response);
        return response;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      this.clearSession();
    }

    return null;
  }

  /**
   * Logout user
   */
  async logout(): Promise<void> {
    try {
      if (this.refreshToken) {
        const request: LogoutRequest = { refreshToken: this.refreshToken };
        await firstValueFrom(this.http.post(`${this.apiUrl}/logout`, request));
      }
    } catch (error) {
      console.error('Logout request failed:', error);
    } finally {
      this.clearSession();
      
      // Sign out from Google
      if (window.google) {
        window.google.accounts.id.disableAutoSelect();
      }
    }
  }

  /**
   * Get current user profile
   */
  getCurrentUser(): Observable<UserInfo> {
    return this.http.get<UserInfo>(`${this.apiUrl}/me`);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    // Check if we have an access token and it's valid
    const hasValidToken = !!this.accessToken && this.isTokenValid();
    
    // Check if we have user info
    const hasUser = !!this.currentUser;
    
    return hasValidToken && hasUser;
  }

  /**
   * Check if user has a refresh token (can potentially be authenticated)
   */
  hasRefreshToken(): boolean {
    return !!this.refreshToken;
  }

  /**
   * Get current user
   */
  get currentUser(): UserInfo | null {
    return this.userSubject.value;
  }

  /**
   * Get access token
   */
  get token(): string | null {
    return this.accessToken;
  }

  /**
   * Check if access token is valid (not expired)
   */
  private isTokenValid(): boolean {
    if (!this.tokenExpiryTime) return false;
    return Date.now() < this.tokenExpiryTime;
  }

  /**
   * Set authentication session
   */
  private setSession(response: AuthResponse): void {
    this.accessToken = response.accessToken;
    this.refreshToken = response.refreshToken;
    this.tokenExpiryTime = Date.now() + (response.expiresIn * 1000);
    
    // Store refresh token in localStorage (persistent)
    localStorage.setItem('refreshToken', this.refreshToken);
    
    // Store user info
    this.userSubject.next(response.user);
    sessionStorage.setItem('user', JSON.stringify(response.user));
  }

  /**
   * Clear authentication session
   */
  private clearSession(): void {
    this.accessToken = null;
    this.refreshToken = null;
    this.tokenExpiryTime = null;
    
    localStorage.removeItem('refreshToken');
    sessionStorage.removeItem('user');
    
    this.userSubject.next(null);
  }

  /**
   * Initialize authentication on app start
   */
  private initializeAuth(): void {
    try {
      // Restore refresh token
      this.refreshToken = localStorage.getItem('refreshToken');
      
      // Restore user info
      const userJson = sessionStorage.getItem('user');
      if (userJson) {
        try {
          const user = JSON.parse(userJson);
          this.userSubject.next(user);
        } catch (error) {
          console.error('Failed to parse stored user info:', error);
          sessionStorage.removeItem('user'); // Remove corrupted data
        }
      }

      // Try to refresh token if we have a refresh token but no access token
      if (this.refreshToken && !this.accessToken) {
        this.refresh().catch((error) => {
          console.error('Token refresh failed during initialization:', error);
          // If refresh fails, clear everything
          this.clearSession();
        });
      }
    } catch (error) {
      console.error('Failed to initialize authentication:', error);
      this.clearSession();
    }
  }

  /**
   * Load Google Identity Services script
   */
  private loadGoogleIdentityServices(): void {
    if (window.google) {
      return; // Already loaded
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => {
      console.log('Google Identity Services loaded successfully');
    };
    script.onerror = (error) => {
      console.error('Failed to load Google Identity Services:', error);
    };
    
    document.head.appendChild(script);
  }
} 