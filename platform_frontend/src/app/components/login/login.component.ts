import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-header">
          <i class="fab fa-google google-icon"></i>
          <h2>Sign in to ITZone</h2>
          <p class="auth-subtitle">Use your Google account to continue</p>
        </div>

        <div class="google-signin-section">
          <button 
            type="button" 
            class="btn btn-google" 
            (click)="signInWithGoogle()" 
            [disabled]="isLoading">
            <i class="fab fa-google"></i>
            <span>{{ isLoading ? 'Signing in...' : 'Continue with Google' }}</span>
          </button>
        </div>

        <div class="error-message" *ngIf="errorMessage">
          <i class="fas fa-exclamation-triangle"></i> 
          {{ errorMessage }}
        </div>

        <div class="auth-footer">
          <p class="security-note">
            <i class="fas fa-shield-alt"></i>
            Secure authentication powered by Google
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }
    
    .auth-card {
      background: white;
      padding: 48px 40px;
      border-radius: 16px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.1);
      width: 100%;
      max-width: 420px;
      text-align: center;
    }
    
    .auth-header {
      margin-bottom: 32px;
    }
    
    .google-icon {
      font-size: 48px;
      color: #ea4335;
      margin-bottom: 16px;
      display: block;
    }
    
    h2 {
      margin: 0 0 8px 0;
      color: #1a1a1a;
      font-size: 28px;
      font-weight: 600;
    }
    
    .auth-subtitle {
      color: #666;
      font-size: 16px;
      margin: 0;
    }
    
    .google-signin-section {
      margin: 32px 0;
    }
    
    .btn {
      width: 100%;
      padding: 16px 24px;
      border: none;
      border-radius: 12px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      position: relative;
      overflow: hidden;
    }
    
    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    
    .btn-google {
      background: #fff;
      color: #1a1a1a;
      border: 2px solid #e0e0e0;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }
    
    .btn-google:hover:not(:disabled) {
      background: #f8f9fa;
      border-color: #dadce0;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      transform: translateY(-1px);
    }
    
    .btn-google:active {
      transform: translateY(0);
    }
    
    .btn-google i {
      color: #ea4335;
      font-size: 20px;
    }
    
    .error-message {
      background: #fef2f2;
      color: #dc2626;
      padding: 12px 16px;
      border-radius: 8px;
      margin-top: 16px;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      border: 1px solid #fecaca;
    }
    
    .auth-footer {
      margin-top: 32px;
      padding-top: 24px;
      border-top: 1px solid #e5e7eb;
    }
    
    .security-note {
      color: #6b7280;
      font-size: 14px;
      margin: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
    }
    
    .security-note i {
      color: #10b981;
    }
    
    @media (max-width: 480px) {
      .auth-container {
        padding: 16px;
      }
      
      .auth-card {
        padding: 32px 24px;
      }
      
      h2 {
        font-size: 24px;
      }
      
      .google-icon {
        font-size: 40px;
      }
    }
  `]
})
export class LoginComponent implements OnInit {
  isLoading = false;
  errorMessage = '';
  private returnUrl = '/';

  constructor(
    private authService: AuthService, 
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Get return URL from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    
    // If user is already authenticated, redirect to return URL
    if (this.authService.isAuthenticated()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  /**
   * Sign in with Google
   */
  async signInWithGoogle() {
    if (this.isLoading) return;
    
    this.isLoading = true;
    this.errorMessage = '';
    
    try {
      await this.authService.signInWithGoogle();
      
      // Redirect to return URL on successful authentication
      this.router.navigateByUrl(this.returnUrl);
    } catch (error: any) {
      console.error('Google sign-in failed:', error);
      
      // Handle specific error types
      if (error.message?.includes('popup_blocked')) {
        this.errorMessage = 'Please allow popups for this site and try again.';
      } else if (error.message?.includes('popup_closed')) {
        this.errorMessage = 'Sign-in was cancelled. Please try again.';
      } else if (error.message?.includes('network')) {
        this.errorMessage = 'Network error. Please check your connection and try again.';
      } else if (error.message?.includes('Email not verified')) {
        this.errorMessage = 'Please verify your email address with Google first.';
      } else if (error.message?.includes('Google Identity Services not loaded')) {
        this.errorMessage = 'Google services are not available. Please refresh the page and try again.';
      } else if (error.message?.includes('Authentication failed')) {
        this.errorMessage = 'Authentication failed. Please try again or contact support.';
      } else if (error.status === 401) {
        this.errorMessage = 'Invalid credentials. Please try again.';
      } else if (error.status === 403) {
        this.errorMessage = 'Access denied. Your account may not have permission to access this application.';
      } else if (error.status >= 500) {
        this.errorMessage = 'Server error. Please try again later.';
      } else {
        this.errorMessage = 'Sign-in failed. Please try again.';
      }
    } finally {
      this.isLoading = false;
    }
  }
} 