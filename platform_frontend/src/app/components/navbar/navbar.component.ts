import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <a [routerLink]="['/']" class="brand-link">
            <i class="fas fa-video"></i>
            <span>منصة الميتنجات الذكية</span>
          </a>
        </div>

        <div class="navbar-menu">
          <a [routerLink]="['/meetings']" class="nav-link" routerLinkActive="active">
            <i class="fas fa-calendar"></i>
            الميتنجات
          </a>
          <a [routerLink]="['/meetings/new']" class="nav-link" routerLinkActive="active">
            <i class="fas fa-plus"></i>
            ميتنج جديد
          </a>

          <a [routerLink]="['/integrations']" class="nav-link" routerLinkActive="active">
            <i class="fas fa-plug"></i>
            التكاملات
          </a>
          <a [routerLink]="['/dashboard']" class="nav-link" routerLinkActive="active">
            <i class="fas fa-chart-bar"></i>
            لوحة التحكم
          </a>
        </div>

        <div class="navbar-user">
          <ng-container *ngIf="auth.user$ | async as user; else guestLinks">
            <div class="user-info">
              <img *ngIf="user.photoUrl" [src]="user.photoUrl" class="user-avatar" alt="User Avatar">
              <i *ngIf="!user.photoUrl" class="fas fa-user-circle"></i>
              <span>{{ user.displayName }}</span>
            </div>
            <button class="btn btn-outline" (click)="logout()">
              <i class="fas fa-sign-out-alt"></i>
              Sign Out
            </button>
          </ng-container>
          <ng-template #guestLinks>
            <a [routerLink]="['/login']" class="btn btn-primary">
              <i class="fab fa-google"></i>
              Sign In with Google
            </a>
          </ng-template>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: white;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .navbar-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 2rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 70px;
    }

    .navbar-brand {
      display: flex;
      align-items: center;
    }

    .brand-link {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      text-decoration: none;
      color: #2c3e50;
      font-size: 1.25rem;
      font-weight: 600;
    }

    .brand-link i {
      color: #007bff;
      font-size: 1.5rem;
    }

    .navbar-menu {
      display: flex;
      align-items: center;
      gap: 2rem;
    }

    .nav-link {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      color: #666;
      font-weight: 500;
      padding: 0.5rem 1rem;
      border-radius: 8px;
      transition: all 0.2s;
    }

    .nav-link:hover {
      color: #007bff;
      background: #f8f9fa;
    }

    .nav-link.active {
      color: #007bff;
      background: #e3f2fd;
    }

    .navbar-user {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      color: #666;
    }

    .user-info i {
      font-size: 1.5rem;
      color: #007bff;
    }

    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      object-fit: cover;
      border: 2px solid #007bff;
    }

    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.9rem;
      transition: all 0.2s;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
    }

    .btn-outline {
      background: transparent;
      color: #666;
      border: 1px solid #ddd;
    }

    .btn-outline:hover {
      background: #f8f9fa;
      color: #d32f2f;
      border-color: #d32f2f;
    }

    .btn-primary {
      background: #4285f4;
      color: white;
      border: 1px solid #4285f4;
    }

    .btn-primary:hover {
      background: #3367d6;
      border-color: #3367d6;
    }

    @media (max-width: 768px) {
      .navbar-container {
        padding: 0 1rem;
        flex-wrap: wrap;
        height: auto;
        padding: 1rem;
      }

      .navbar-menu {
        order: 3;
        width: 100%;
        justify-content: center;
        margin-top: 1rem;
        gap: 1rem;
      }

      .navbar-user {
        order: 2;
      }

      .brand-link span {
        display: none;
      }
    }
  `]
})
export class NavbarComponent {
  constructor(public auth: AuthService, private router: Router) {}

  async logout(): Promise<void> {
    try {
      await this.auth.logout();
      this.router.navigateByUrl('/login');
    } catch (error) {
      console.error('Logout failed:', error);
      // Still redirect to login even if logout request fails
      this.router.navigateByUrl('/login');
    }
  }
} 