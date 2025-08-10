import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="redirect-message">
          <i class="fab fa-google google-icon"></i>
          <h2>Welcome to ITZone</h2>
          <p>We use Google-only authentication for security and convenience.</p>
          <p>Redirecting you to sign in...</p>
          <div class="loading-spinner">
            <i class="fas fa-spinner fa-spin"></i>
          </div>
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
    
    .redirect-message {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
    }
    
    .google-icon {
      font-size: 48px;
      color: #ea4335;
    }
    
    h2 {
      margin: 0;
      color: #1a1a1a;
      font-size: 28px;
      font-weight: 600;
    }
    
    p {
      color: #666;
      font-size: 16px;
      margin: 0;
      line-height: 1.5;
    }
    
    .loading-spinner {
      margin-top: 16px;
    }
    
    .loading-spinner i {
      font-size: 24px;
      color: #4285f4;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .fa-spin {
      animation: spin 1s linear infinite;
    }
  `]
})
export class RegisterComponent implements OnInit {
  constructor(private router: Router) {}

  ngOnInit() {
    // Redirect to login since we only support Google authentication
    setTimeout(() => {
      this.router.navigateByUrl('/login');
    }, 2000);
  }
} 