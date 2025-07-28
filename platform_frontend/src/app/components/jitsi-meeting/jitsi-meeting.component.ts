import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ExternalIntegrationService } from '../../services/external-integration.service';
import { CreateJitsiMeetingDto, JitsiMeeting } from '../../models/external-integration.model';

@Component({
  selector: 'app-jitsi-meeting',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="jitsi-meeting-container">
      <div class="header">
        <h2><i class="fas fa-video"></i> إنشاء ميتنج Jitsi</h2>
        <p>قم بإنشاء ميتنج جديد مع إمكانية التسجيل والتحليل التلقائي</p>
      </div>

      <div class="meeting-form" *ngIf="!currentMeeting">
        <form (ngSubmit)="createMeeting()" #meetingForm="ngForm">
          <div class="form-group">
            <label for="roomName">اسم الغرفة:</label>
            <input 
              type="text" 
              id="roomName" 
              name="roomName"
              [(ngModel)]="meetingData.roomName"
              placeholder="أدخل اسم الغرفة (اختياري)"
              class="form-control">
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="startTime">وقت البداية:</label>
              <input 
                type="datetime-local" 
                id="startTime" 
                name="startTime"
                [(ngModel)]="meetingData.startTime"
                required
                class="form-control">
            </div>

            <div class="form-group">
              <label for="endTime">وقت النهاية:</label>
              <input 
                type="datetime-local" 
                id="endTime" 
                name="endTime"
                [(ngModel)]="meetingData.endTime"
                required
                class="form-control">
            </div>
          </div>

          <div class="form-group">
            <label for="participants">المشاركون (البريد الإلكتروني):</label>
            <textarea 
              id="participants" 
              name="participants"
              [(ngModel)]="participantsText"
              placeholder="أدخل عناوين البريد الإلكتروني مفصولة بفواصل"
              class="form-control"
              rows="3"></textarea>
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input 
                type="checkbox" 
                name="enableRecording"
                [(ngModel)]="meetingData.enableRecording">
              <span class="checkmark"></span>
              تفعيل التسجيل التلقائي
            </label>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn btn-primary" [disabled]="isLoading">
              <i class="fas fa-plus"></i>
              {{ isLoading ? 'جاري الإنشاء...' : 'إنشاء الميتنج' }}
            </button>
          </div>
        </form>
      </div>

      <div class="meeting-details" *ngIf="currentMeeting">
        <div class="meeting-info">
          <h3><i class="fas fa-video"></i> {{ currentMeeting.roomName || 'ميتنج Jitsi' }}</h3>
          <div class="meeting-stats">
            <div class="stat">
              <i class="fas fa-clock"></i>
              <span>وقت البداية: {{ currentMeeting.startTime | date:'short' }}</span>
            </div>
            <div class="stat">
              <i class="fas fa-stop-circle"></i>
              <span>وقت النهاية: {{ currentMeeting.endTime | date:'short' }}</span>
            </div>
            <div class="stat">
              <i class="fas fa-users"></i>
              <span>عدد المشاركين: {{ (currentMeeting.participantEmails?.length) || 0 }}</span>
            </div>
            <div class="stat">
              <i class="fas fa-record-vinyl" [class.recording]="currentMeeting.isRecording"></i>
              <span>{{ currentMeeting.isRecording ? 'جاري التسجيل' : 'غير مسجل' }}</span>
            </div>
          </div>
        </div>

        <div class="meeting-actions">
          <a [href]="currentMeeting.jitsiUrl" target="_blank" class="btn btn-success">
            <i class="fas fa-external-link-alt"></i>
            انضم للميتنج
          </a>

          <button 
            (click)="startRecording()" 
            class="btn btn-warning"
            [disabled]="currentMeeting.isRecording || isRecordingLoading">
            <i class="fas fa-record-vinyl"></i>
            {{ isRecordingLoading ? 'جاري البدء...' : 'بدء التسجيل' }}
          </button>

          <button 
            (click)="stopRecording()" 
            class="btn btn-danger"
            [disabled]="!currentMeeting.isRecording || isRecordingLoading">
            <i class="fas fa-stop"></i>
            {{ isRecordingLoading ? 'جاري الإيقاف...' : 'إيقاف التسجيل' }}
          </button>

          <button (click)="resetMeeting()" class="btn btn-secondary">
            <i class="fas fa-plus"></i>
            ميتنج جديد
          </button>
        </div>

        <div class="recording-info" *ngIf="currentMeeting.recordingUrl">
          <h4><i class="fas fa-file-audio"></i> معلومات التسجيل</h4>
          <div class="recording-url">
            <strong>رابط التسجيل:</strong>
            <a [href]="currentMeeting.recordingUrl" target="_blank" class="recording-link">
              {{ currentMeeting.recordingUrl }}
            </a>
          </div>
          <div class="recording-actions">
            <button (click)="uploadRecording()" class="btn btn-info">
              <i class="fas fa-upload"></i>
              رفع التسجيل للتحليل
            </button>
            <button (click)="processRecordingAutomatically()" class="btn btn-success">
              <i class="fas fa-magic"></i>
              معالجة تلقائية وإرسال الملخص
            </button>
          </div>
        </div>
      </div>

      <div class="error-message" *ngIf="errorMessage">
        <i class="fas fa-exclamation-triangle"></i>
        {{ errorMessage }}
      </div>

      <div class="success-message" *ngIf="successMessage">
        <i class="fas fa-check-circle"></i>
        {{ successMessage }}
      </div>
    </div>
  `,
  styles: [`
    .jitsi-meeting-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .header {
      text-align: center;
      margin-bottom: 30px;
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

    .meeting-form {
      background: #f8f9fa;
      padding: 25px;
      border-radius: 8px;
      margin-bottom: 20px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
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
      transition: border-color 0.3s ease;
    }

    .form-control:focus {
      outline: none;
      border-color: #3498db;
      box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      cursor: pointer;
      font-weight: 500;
    }

    .checkbox-label input[type="checkbox"] {
      margin-left: 10px;
      transform: scale(1.2);
    }

    .form-actions {
      text-align: center;
      margin-top: 30px;
    }

    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      margin: 0 5px;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 8px;
    }

    .btn-primary {
      background: #3498db;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #2980b9;
      transform: translateY(-2px);
    }

    .btn-success {
      background: #27ae60;
      color: white;
    }

    .btn-warning {
      background: #f39c12;
      color: white;
    }

    .btn-danger {
      background: #e74c3c;
      color: white;
    }

    .btn-secondary {
      background: #95a5a6;
      color: white;
    }

    .btn-info {
      background: #17a2b8;
      color: white;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }

    .meeting-details {
      background: #f8f9fa;
      padding: 25px;
      border-radius: 8px;
    }

    .meeting-info h3 {
      color: #2c3e50;
      margin-bottom: 20px;
      text-align: center;
    }

    .meeting-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 15px;
      margin-bottom: 25px;
    }

    .stat {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 15px;
      background: white;
      border-radius: 6px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .stat i {
      color: #3498db;
      font-size: 18px;
    }

    .stat i.recording {
      color: #e74c3c;
      animation: pulse 1.5s infinite;
    }

    @keyframes pulse {
      0% { opacity: 1; }
      50% { opacity: 0.5; }
      100% { opacity: 1; }
    }

    .meeting-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      justify-content: center;
      margin-bottom: 25px;
    }

    .recording-info {
      background: white;
      padding: 20px;
      border-radius: 6px;
      border-left: 4px solid #3498db;
    }

    .recording-info h4 {
      color: #2c3e50;
      margin-bottom: 15px;
    }

    .recording-url {
      margin-bottom: 15px;
    }

    .recording-link {
      color: #3498db;
      text-decoration: none;
      word-break: break-all;
    }

    .recording-link:hover {
      text-decoration: underline;
    }

    .recording-actions {
      text-align: center;
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

    .success-message {
      background: #d4edda;
      color: #155724;
      padding: 15px;
      border-radius: 6px;
      margin-top: 20px;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    @media (max-width: 768px) {
      .form-row {
        grid-template-columns: 1fr;
      }

      .meeting-stats {
        grid-template-columns: 1fr;
      }

      .meeting-actions {
        flex-direction: column;
      }

      .btn {
        width: 100%;
        justify-content: center;
      }
    }
  `]
})
export class JitsiMeetingComponent implements OnInit {
  meetingData: CreateJitsiMeetingDto = {
    roomName: '',
    startTime: new Date(),
    endTime: new Date(Date.now() + 60 * 60 * 1000), // ساعة من الآن
    participantEmails: [],
    enableRecording: true
  };

  participantsText = '';
  currentMeeting: JitsiMeeting | null = null;
  isLoading = false;
  isRecordingLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(private externalService: ExternalIntegrationService) { }

  ngOnInit() {
    // Set default times
    const now = new Date();
    const oneHourLater = new Date(now.getTime() + 60 * 60 * 1000);
    
    this.meetingData.startTime = now;
    this.meetingData.endTime = oneHourLater;
  }

  createMeeting() {
    if (this.isLoading) return;

    // تحويل النص إلى مصفوفة
    this.meetingData.participantEmails = this.participantsText
      .split(',')
      .map(email => email.trim())
      .filter(email => email.length > 0);

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.externalService.createJitsiMeeting(this.meetingData).subscribe({
      next: (meeting) => {
        this.currentMeeting = meeting;
        this.isLoading = false;
        this.successMessage = 'تم إنشاء الميتنج بنجاح!';
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء إنشاء الميتنج: ' + error.message;
        this.isLoading = false;
      }
    });
  }

  startRecording() {
    if (!this.currentMeeting || this.isRecordingLoading) return;

    this.isRecordingLoading = true;
    this.externalService.startRecording(this.currentMeeting.id).subscribe({
      next: (success) => {
        if (success && this.currentMeeting) {
          this.currentMeeting.isRecording = true;
        }
        this.isRecordingLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء بدء التسجيل: ' + error.message;
        this.isRecordingLoading = false;
      }
    });
  }

  stopRecording() {
    if (!this.currentMeeting || this.isRecordingLoading) return;

    this.isRecordingLoading = true;
    this.externalService.stopRecording(this.currentMeeting.id).subscribe({
      next: (success) => {
        if (success && this.currentMeeting) {
          this.currentMeeting.isRecording = false;
          // تحديث رابط التسجيل
          this.externalService.getRecordingUrl(this.currentMeeting.id).subscribe({
            next: (url) => {
              if (this.currentMeeting) {
                this.currentMeeting.recordingUrl = url;
              }
            }
          });
        }
        this.isRecordingLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء إيقاف التسجيل: ' + error.message;
        this.isRecordingLoading = false;
      }
    });
  }

  uploadRecording() {
    // سيتم تنفيذ رفع التسجيل هنا
    console.log('رفع التسجيل للتحليل');
  }

  processRecordingAutomatically() {
    if (!this.currentMeeting || this.isRecordingLoading) return;

    this.isRecordingLoading = true;
    this.externalService.processRecordingAutomatically(this.currentMeeting.id).subscribe({
      next: (summary) => {
        this.isRecordingLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'حدث خطأ أثناء معالجة التسجيل تلقائياً: ' + error.message;
        this.isRecordingLoading = false;
      }
    });
  }

  resetMeeting() {
    this.currentMeeting = null;
    this.meetingData = {
      roomName: '',
      startTime: new Date(),
      endTime: new Date(Date.now() + 60 * 60 * 1000),
      participantEmails: [],
      enableRecording: true
    };
    this.participantsText = '';
    this.errorMessage = '';
    this.successMessage = '';
  }
} 