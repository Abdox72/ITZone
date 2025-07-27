import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExternalIntegrationService } from '../../services/external-integration.service';
import { AuthService } from '../../services/auth.service';
import { ExternalIntegration } from '../../models/external-integration.model';

@Component({
  selector: 'app-external-integrations',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="integrations-container">
      <div class="header">
        <h2><i class="fas fa-plug"></i> التكاملات الخارجية</h2>
        <p>قم بربط حسابك مع الخدمات الخارجية لإرسال المهام والتحليلات تلقائياً</p>
        
        <div class="user-info" *ngIf="auth.currentUser">
          <div class="user-avatar">
            <img *ngIf="auth.currentUser.avatar" [src]="auth.currentUser.avatar" alt="User Avatar">
            <i *ngIf="!auth.currentUser.avatar" class="fas fa-user-circle"></i>
          </div>
          <div class="user-details">
            <h4>{{ auth.currentUser.name }}</h4>
            <p>{{ auth.currentUser.email }}</p>
          </div>
        </div>
      </div>

      <div class="auto-processing-info">
        <div class="info-card">
          <i class="fas fa-magic"></i>
          <h4>المعالجة التلقائية</h4>
          <p>بعد ربط الخدمات، سيتم إرسال ملخص الميتنج والمهام تلقائياً إلى حسابك</p>
        </div>
      </div>

      <div class="integrations-grid">
        <!-- Gmail Integration -->
        <div class="integration-card" [class.connected]="isConnected('Gmail')">
          <div class="integration-header">
            <div class="integration-icon gmail">
              <i class="fas fa-envelope"></i>
            </div>
            <div class="integration-info">
              <h3>Gmail</h3>
              <p>إرسال تحليلات الميتنجات والمهام عبر البريد الإلكتروني</p>
              <div class="integration-details" *ngIf="isConnected('Gmail')">
                <small>سيتم الإرسال إلى: {{ getConnectedEmail('Gmail') }}</small>
              </div>
            </div>
            <div class="integration-status">
              <span class="status-badge" [class.connected]="isConnected('Gmail')">
                {{ isConnected('Gmail') ? 'متصل' : 'غير متصل' }}
              </span>
            </div>
          </div>
          
          <div class="integration-actions">
            <button 
              *ngIf="!isConnected('Gmail')"
              (click)="connectService('Gmail')" 
              class="btn btn-primary"
              [disabled]="isConnecting">
              <i class="fas fa-link"></i>
              {{ isConnecting ? 'جاري الاتصال...' : 'ربط الحساب' }}
            </button>
            <button 
              *ngIf="isConnected('Gmail')"
              (click)="disconnectService('Gmail')" 
              class="btn btn-danger">
              <i class="fas fa-unlink"></i>
              قطع الاتصال
            </button>
          </div>
        </div>

        <!-- Trello Integration -->
        <div class="integration-card" [class.connected]="isConnected('Trello')">
          <div class="integration-header">
            <div class="integration-icon trello">
              <i class="fab fa-trello"></i>
            </div>
            <div class="integration-info">
              <h3>Trello</h3>
              <p>إنشاء المهام تلقائياً في لوحات Trello</p>
              <div class="integration-details" *ngIf="isConnected('Trello')">
                <small>سيتم إنشاء المهام في لوحة: {{ getConnectedEmail('Trello') }}</small>
              </div>
            </div>
            <div class="integration-status">
              <span class="status-badge" [class.connected]="isConnected('Trello')">
                {{ isConnected('Trello') ? 'متصل' : 'غير متصل' }}
              </span>
            </div>
          </div>
          
          <div class="integration-actions">
            <button 
              *ngIf="!isConnected('Trello')"
              (click)="connectService('Trello')" 
              class="btn btn-primary"
              [disabled]="isConnecting">
              <i class="fas fa-link"></i>
              {{ isConnecting ? 'جاري الاتصال...' : 'ربط الحساب' }}
            </button>
            <button 
              *ngIf="isConnected('Trello')"
              (click)="disconnectService('Trello')" 
              class="btn btn-danger">
              <i class="fas fa-unlink"></i>
              قطع الاتصال
            </button>
          </div>
        </div>

        <!-- Google Calendar Integration -->
        <div class="integration-card" [class.connected]="isConnected('GoogleCalendar')">
          <div class="integration-header">
            <div class="integration-icon calendar">
              <i class="fas fa-calendar-alt"></i>
            </div>
            <div class="integration-info">
              <h3>Google Calendar</h3>
              <p>إضافة المهام كأحداث في التقويم</p>
              <div class="integration-details" *ngIf="isConnected('GoogleCalendar')">
                <small>سيتم إضافة الأحداث إلى: {{ getConnectedEmail('GoogleCalendar') }}</small>
              </div>
            </div>
            <div class="integration-status">
              <span class="status-badge" [class.connected]="isConnected('GoogleCalendar')">
                {{ isConnected('GoogleCalendar') ? 'متصل' : 'غير متصل' }}
              </span>
            </div>
          </div>
          
          <div class="integration-actions">
            <button 
              *ngIf="!isConnected('GoogleCalendar')"
              (click)="connectService('GoogleCalendar')" 
              class="btn btn-primary"
              [disabled]="isConnecting">
              <i class="fas fa-link"></i>
              {{ isConnecting ? 'جاري الاتصال...' : 'ربط الحساب' }}
            </button>
            <button 
              *ngIf="isConnected('GoogleCalendar')"
              (click)="disconnectService('GoogleCalendar')" 
              class="btn btn-danger">
              <i class="fas fa-unlink"></i>
              قطع الاتصال
            </button>
          </div>
        </div>
      </div>

      <div class="error-message" *ngIf="errorMessage">
        <i class="fas fa-exclamation-triangle"></i>
        {{ errorMessage }}
      </div>
    </div>
  `,
  styles: [`
    .integrations-container {
      max-width: 1000px;
      margin: 0 auto;
      padding: 20px;
    }

    .header {
      text-align: center;
      margin-bottom: 40px;
      padding-bottom: 20px;
      border-bottom: 2px solid #f0f0f0;
    }

    .header h2 {
      color: #2c3e50;
      margin-bottom: 10px;
    }

    .header p {
      color: #7f8c8d;
      font-size: 16px;
    }

    .user-info {
      display: flex;
      align-items: center;
      gap: 15px;
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid #eee;
    }

    .user-avatar {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      overflow: hidden;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: #e0e0e0;
    }

    .user-avatar img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .user-avatar i {
      font-size: 24px;
      color: #555;
    }

    .user-details h4 {
      margin: 0;
      color: #34495e;
      font-size: 18px;
    }

    .user-details p {
      margin: 0;
      color: #7f8c8d;
      font-size: 14px;
    }

    .auto-processing-info {
      background: #f8f9fa;
      padding: 25px;
      border-radius: 12px;
      margin-bottom: 20px;
      text-align: center;
    }

    .info-card {
      display: flex;
      align-items: center;
      gap: 15px;
      padding: 15px;
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .info-card i {
      color: #3498db;
      font-size: 24px;
    }

    .info-card h4 {
      color: #2c3e50;
      margin-bottom: 5px;
      font-size: 18px;
    }

    .info-card p {
      color: #7f8c8d;
      font-size: 14px;
      margin: 0;
      line-height: 1.4;
    }

    .integrations-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      margin-bottom: 40px;
    }

    .integration-card {
      background: white;
      border: 2px solid #e9ecef;
      border-radius: 12px;
      padding: 25px;
      transition: all 0.3s ease;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .integration-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
    }

    .integration-card.connected {
      border-color: #27ae60;
      background: linear-gradient(135deg, #f8fff9 0%, #ffffff 100%);
    }

    .integration-header {
      display: flex;
      align-items: center;
      gap: 15px;
      margin-bottom: 20px;
    }

    .integration-icon {
      width: 50px;
      height: 50px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
      color: white;
    }

    .integration-icon.gmail {
      background: linear-gradient(135deg, #ea4335 0%, #d93025 100%);
    }

    .integration-icon.trello {
      background: linear-gradient(135deg, #0079bf 0%, #005a8b 100%);
    }

    .integration-icon.calendar {
      background: linear-gradient(135deg, #4285f4 0%, #3367d6 100%);
    }

    .integration-info {
      flex: 1;
    }

    .integration-info h3 {
      color: #2c3e50;
      margin-bottom: 5px;
      font-size: 18px;
    }

    .integration-info p {
      color: #7f8c8d;
      font-size: 14px;
      margin: 0;
    }

    .integration-details {
      margin-top: 10px;
      color: #555;
      font-size: 13px;
    }

    .integration-status {
      text-align: left;
    }

    .status-badge {
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      background: #f8f9fa;
      color: #6c757d;
      border: 1px solid #dee2e6;
    }

    .status-badge.connected {
      background: #d4edda;
      color: #155724;
      border-color: #c3e6cb;
    }

    .integration-actions {
      text-align: center;
    }

    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      display: inline-flex;
      align-items: center;
      gap: 8px;
      text-decoration: none;
    }

    .btn-primary {
      background: #3498db;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #2980b9;
      transform: translateY(-1px);
    }

    .btn-danger {
      background: #e74c3c;
      color: white;
    }

    .btn-danger:hover {
      background: #c0392b;
      transform: translateY(-1px);
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    .error-message {
      background: #f8d7da;
      color: #721c24;
      padding: 15px;
      border-radius: 6px;
      margin-top: 20px;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    @media (max-width: 768px) {
      .integrations-grid {
        grid-template-columns: 1fr;
      }

      .user-info {
        flex-direction: column;
        text-align: center;
      }

      .user-avatar {
        margin-bottom: 10px;
      }

      .info-card {
        flex-direction: column;
        text-align: center;
      }

      .info-card i {
        margin-bottom: 10px;
      }
    }
  `]
})
export class ExternalIntegrationsComponent implements OnInit {
  integrations: ExternalIntegration[] = [];
  isConnecting = false;
  errorMessage = '';

  constructor(public externalService: ExternalIntegrationService, public auth: AuthService) { }

  ngOnInit() {
    this.loadIntegrations();
  }

  loadIntegrations() {
    this.externalService.getUserIntegrations().subscribe({
      next: (integrations) => {
        this.integrations = integrations;
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء تحميل التكاملات: ' + error.message;
      }
    });
  }

  isConnected(platform: string): boolean {
    return this.integrations.some(integration => 
      integration.platform === platform && integration.isActive
    );
  }

  getConnectedEmail(platform: string): string {
    const integration = this.integrations.find(i => i.platform === platform);
    if (integration) {
      return integration.connectedEmail || integration.userEmail || 'غير محدد';
    }
    return 'غير محدد';
  }

  connectService(platform: string) {
    this.isConnecting = true;
    this.errorMessage = '';

    this.externalService.getAuthorizationUrl(platform).subscribe({
      next: (authUrl) => {
        // في التطبيق الحقيقي، سيتم فتح نافذة OAuth
        window.open(authUrl, '_blank', 'width=500,height=600');
        this.isConnecting = false;
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء الحصول على رابط التصريح: ' + error.message;
        this.isConnecting = false;
      }
    });
  }

  disconnectService(platform: string) {
    const integration = this.integrations.find(i => i.platform === platform);
    if (!integration) return;

    this.externalService.disconnectExternalService(integration.id).subscribe({
      next: (success) => {
        if (success) {
          this.loadIntegrations(); // إعادة تحميل التكاملات
        }
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء قطع الاتصال: ' + error.message;
      }
    });
  }
} 