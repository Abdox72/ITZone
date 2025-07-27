import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService, RegisterDto, GoogleLoginDto } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>تسجيل حساب جديد</h2>
        <form (ngSubmit)="register()" #registerForm="ngForm">
          <div class="form-group">
            <label for="name">الاسم الكامل</label>
            <input type="text" id="name" name="name" [(ngModel)]="registerData.name" required class="form-control">
          </div>
          <div class="form-group">
            <label for="email">البريد الإلكتروني</label>
            <input type="email" id="email" name="email" [(ngModel)]="registerData.email" required class="form-control">
          </div>
          <div class="form-group">
            <label for="password">كلمة المرور</label>
            <input type="password" id="password" name="password" [(ngModel)]="registerData.password" required class="form-control">
          </div>
          <button type="submit" class="btn btn-primary" [disabled]="isLoading">{{ isLoading ? 'جاري التسجيل...' : 'تسجيل' }}</button>
        </form>
        <div class="divider">أو</div>
        <button class="btn btn-google" (click)="googleSignUp()">
          <i class="fab fa-google"></i> التسجيل عبر Google
        </button>
        <div class="auth-footer">
          <span>لديك حساب بالفعل؟</span>
          <a [routerLink]="['/login']">سجّل الدخول</a>
        </div>
        <div class="error-message" *ngIf="errorMessage">
          <i class="fas fa-exclamation-triangle"></i> {{ errorMessage }}
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
      background: #f8f9fa;
    }
    .auth-card {
      background: white;
      padding: 40px 30px;
      border-radius: 12px;
      box-shadow: 0 4px 16px rgba(0,0,0,0.08);
      width: 100%;
      max-width: 400px;
      text-align: center;
    }
    h2 {
      margin-bottom: 30px;
      color: #2c3e50;
    }
    .form-group {
      margin-bottom: 20px;
      text-align: right;
    }
    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-weight: 600;
      color: #2c3e50;
    }
    .form-control {
      width: 100%;
      padding: 12px;
      border: 2px solid #e9ecef;
      border-radius: 6px;
      font-size: 14px;
      transition: border-color 0.3s;
    }
    .form-control:focus {
      border-color: #3498db;
      outline: none;
    }
    .btn {
      width: 100%;
      padding: 12px;
      border: none;
      border-radius: 6px;
      font-size: 15px;
      font-weight: 600;
      margin-bottom: 10px;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-primary {
      background: #3498db;
      color: white;
    }
    .btn-primary:hover {
      background: #2980b9;
    }
    .btn-google {
      background: #fff;
      color: #444;
      border: 1px solid #ddd;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }
    .btn-google i {
      color: #ea4335;
      font-size: 18px;
    }
    .btn-google:hover {
      background: #f5f5f5;
    }
    .divider {
      margin: 20px 0;
      color: #aaa;
      font-size: 14px;
      position: relative;
    }
    .divider:before, .divider:after {
      content: '';
      display: inline-block;
      width: 40%;
      height: 1px;
      background: #e0e0e0;
      vertical-align: middle;
      margin: 0 8px;
    }
    .auth-footer {
      margin-top: 18px;
      font-size: 14px;
    }
    .auth-footer a {
      color: #3498db;
      text-decoration: none;
      margin-right: 5px;
    }
    .auth-footer a:hover {
      text-decoration: underline;
    }
    .error-message {
      background: #f8d7da;
      color: #721c24;
      padding: 10px;
      border-radius: 6px;
      margin-top: 15px;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
    }
  `]
})
export class RegisterComponent {
  registerData: RegisterDto = { name: '', email: '', password: '' };
  isLoading = false;
  errorMessage = '';

  constructor(private auth: AuthService, private router: Router) {}

  register() {
    if (this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';
    this.auth.register(this.registerData).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl('/');
      },
      error: err => {
        this.errorMessage = err.error?.error || 'فشل التسجيل';
        this.isLoading = false;
      }
    });
  }

  googleSignUp() {
    // في التطبيق الحقيقي: استخدم مكتبة Google Sign-In
    // هنا: محاكاة تسجيل دخول Google
    const fakeIdToken = 'dummy-google-id-token';
    this.isLoading = true;
    this.auth.googleLogin({ idToken: fakeIdToken }).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl('/');
      },
      error: err => {
        this.errorMessage = err.error?.error || 'فشل التسجيل عبر Google';
        this.isLoading = false;
      }
    });
  }
} 