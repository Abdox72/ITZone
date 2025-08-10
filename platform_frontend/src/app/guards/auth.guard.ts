import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn, CanMatchFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { firstValueFrom } from 'rxjs';

/**
 * Auth Guard - Protects routes that require authentication
 * Redirects to login page if user is not authenticated
 */
export const AuthGuard: CanActivateFn = async (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If already authenticated, allow access
  if (authService.isAuthenticated()) {
    return true;
  }

  // If we have a refresh token, try to refresh
  if (authService.hasRefreshToken()) {
    try {
      const refreshResult = await authService.refresh();
      if (refreshResult && authService.isAuthenticated()) {
        return true;
      }
    } catch (error) {
      console.error('Token refresh failed in auth guard:', error);
    }
  }

  // Store the attempted URL for redirecting after login
  router.navigate(['/login'], { 
    queryParams: { returnUrl: state.url } 
  });
  return false;
};

/**
 * Guest Guard - Prevents authenticated users from accessing login/register pages
 * Redirects to home page if user is already authenticated
 */
export const GuestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // User is already authenticated, redirect to home
  router.navigate(['/']);
  return false;
};

/**
 * Can Match Guard for lazy-loaded modules
 */
export const AuthCanMatch: CanMatchFn = (route, segments) => {
  const authService = inject(AuthService);
  return authService.isAuthenticated();
};

export const GuestCanMatch: CanMatchFn = (route, segments) => {
  const authService = inject(AuthService);
  return !authService.isAuthenticated();
};