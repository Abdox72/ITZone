import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take, from, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * HTTP Interceptor for automatic token management
 * - Adds Authorization header to requests
 * - Handles token refresh on 401 errors
 * - Redirects to login on authentication failure
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  // Skip auth for auth endpoints and external URLs
  if (req.url.includes('/auth/') || !req.url.includes('/api/')) {
    return next(req);
  }

  // Add Authorization header if we have a token
  const token = authService.token;
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && token) {
        return handleUnauthorizedError(req, next, authService, router);
      }

      // Handle other errors
      if (error.status === 403) {
        console.warn('Access forbidden - insufficient permissions');
      }

      return throwError(() => error);
    })
  );
};

// Subject to handle concurrent refresh requests
let isRefreshing = false;
let refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

/**
 * Handle 401 Unauthorized errors by attempting token refresh
 */
function handleUnauthorizedError(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
): Observable<any> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return from(authService.refresh()).pipe(
      switchMap(response => {
        isRefreshing = false;
        
        if (response) {
          // Token refresh successful
          refreshTokenSubject.next(response.accessToken);
          
          // Retry the original request with new token
          const newReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${response.accessToken}`
            }
          });
          
          return next(newReq);
        } else {
          // Token refresh failed - redirect to login
          refreshTokenSubject.next(null);
          authService.logout();
          router.navigate(['/login']);
          return throwError(() => new Error('Authentication failed'));
        }
      }),
      catchError(error => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => error);
      })
    );
  } else {
    // Wait for the refresh to complete
    return refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(token => {
        const newReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next(newReq);
      })
    );
  }
}