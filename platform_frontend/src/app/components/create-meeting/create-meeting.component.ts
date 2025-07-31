import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { CreateMeetingDto } from '../../models/meeting.model';
import { CreateJitsiMeetingDto } from '../../models/external-integration.model';

@Component({
  selector: 'app-create-meeting',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="create-meeting-container">
      <div class="form-card">
        <div class="header">
          <h1>إنشاء ميتنج جديد</h1>
          <button class="btn btn-outline" (click)="goBack()">
            <i class="fas fa-arrow-right"></i>
            رجوع
          </button>
        </div>

        <form (ngSubmit)="onSubmit()" #meetingForm="ngForm" class="meeting-form">
          <div class="form-group">
            <label for="title">عنوان الميتنج *</label>
            <input
              type="text"
              id="title"
              name="title"
              [(ngModel)]="meeting.title"
              required
              class="form-control"
              placeholder="أدخل عنوان الميتنج"
            />
          </div>

          <div class="form-group">
            <label for="description">وصف الميتنج</label>
            <textarea
              id="description"
              name="description"
              [(ngModel)]="meeting.description"
              rows="4"
              class="form-control"
              placeholder="أدخل وصف الميتنج"
            ></textarea>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="startTime">وقت البداية *</label>
              <input
                type="datetime-local"
                id="startTime"
                name="startTime"
                [(ngModel)]="startTimeString"
                required
                class="form-control"
              />
            </div>

            <div class="form-group">
              <label for="endTime">وقت النهاية *</label>
              <input
                type="datetime-local"
                id="endTime"
                name="endTime"
                [(ngModel)]="endTimeString"
                required
                class="form-control"
              />
            </div>
          </div>

          <div class="form-group">
            <label for="participants">المشاركون</label>
            <div class="participants-input">
              <input
                type="email"
                name="newParticipant"
                [(ngModel)]="newParticipant"
                (keyup.enter)="addParticipant()"
                class="form-control"
                placeholder="أدخل بريد إلكتروني للمشارك"
              />
              <button type="button" class="btn btn-sm btn-primary" (click)="addParticipant()">
                إضافة
              </button>
            </div>
            
            <div class="participants-list" *ngIf="(meeting.participantEmails?.length || 0) > 0">
              <div class="participant-tag" *ngFor="let email of meeting.participantEmails; let i = index">
                <span>{{ email }}</span>
                <button type="button" class="remove-btn" (click)="removeParticipant(i)">
                  <i class="fas fa-times"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- خيارات الميتنج -->
          <div class="meeting-options">
            <h3>خيارات الميتنج</h3>
            
            <div class="option-group">
              <label class="checkbox-label">
                <input 
                  type="checkbox" 
                  [(ngModel)]="useJitsi"
                  name="useJitsi">
                <span class="checkmark"></span>
                <i class="fas fa-video"></i>
                استخدام Jitsi للميتنج
              </label>
              <p class="option-description">إنشاء ميتنج مع إمكانية التسجيل والتحليل التلقائي</p>
            </div>

            <div class="option-group" *ngIf="useJitsi">
              <label class="checkbox-label">
                <input 
                  type="checkbox" 
                  [(ngModel)]="enableRecording"
                  name="enableRecording">
                <span class="checkmark"></span>
                <i class="fas fa-record-vinyl"></i>
                تفعيل التسجيل التلقائي
              </label>
              <p class="option-description">تسجيل الميتنج تلقائياً للتحليل لاحقاً</p>
            </div>
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-outline" (click)="goBack()">
              إلغاء
            </button>
            <button type="submit" class="btn btn-primary" [disabled]="!meetingForm.valid || isSubmitting">
              <i class="fas fa-save"></i>
              {{ isSubmitting ? 'جاري الإنشاء...' : (useJitsi ? 'إنشاء ميتنج Jitsi' : 'إنشاء الميتنج') }}
            </button>
          </div>
        </form>
      </div>

      <!-- رسائل النجاح والخطأ -->
      <div class="success-message" *ngIf="successMessage">
        <i class="fas fa-check-circle"></i>
        {{ successMessage }}
      </div>

      <div class="error-message" *ngIf="errorMessage">
        <i class="fas fa-exclamation-triangle"></i>
        {{ errorMessage }}
      </div>
    </div>
  `,
  styles: [`
    .create-meeting-container {
      padding: 2rem;
      max-width: 800px;
      margin: 0 auto;
    }

    .form-card {
      background: white;
      border-radius: 12px;
      padding: 2rem;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid #eee;
    }

    .header h1 {
      color: #2c3e50;
      margin: 0;
    }

    .meeting-form {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    label {
      font-weight: 500;
      color: #2c3e50;
    }

    .form-control {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 1rem;
      transition: border-color 0.2s;
    }

    .form-control:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
    }

    .participants-input {
      display: flex;
      gap: 0.5rem;
    }

    .participants-input .form-control {
      flex: 1;
    }

    .participants-list {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }

    .participant-tag {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: #e3f2fd;
      color: #1976d2;
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.9rem;
    }

    .remove-btn {
      background: none;
      border: none;
      color: #1976d2;
      cursor: pointer;
      padding: 0;
      font-size: 0.8rem;
    }

    .remove-btn:hover {
      color: #d32f2f;
    }

    .meeting-options {
      background: #f8f9fa;
      padding: 1.5rem;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .meeting-options h3 {
      color: #2c3e50;
      margin-bottom: 1rem;
      font-size: 1.1rem;
    }

    .option-group {
      margin-bottom: 1rem;
    }

    .option-group:last-child {
      margin-bottom: 0;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      cursor: pointer;
      font-weight: 500;
      color: #2c3e50;
    }

    .checkbox-label input[type="checkbox"] {
      transform: scale(1.2);
      margin: 0;
    }

    .checkbox-label i {
      color: #007bff;
      font-size: 1.1rem;
    }

    .option-description {
      color: #6c757d;
      font-size: 0.9rem;
      margin: 0.5rem 0 0 2rem;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      margin-top: 2rem;
      padding-top: 1rem;
      border-top: 1px solid #eee;
    }

    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 1rem;
      transition: all 0.2s;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn-outline {
      background: transparent;
      color: #666;
      border: 1px solid #ddd;
    }

    .btn-outline:hover {
      background: #f8f9fa;
    }

    .btn-sm {
      padding: 0.5rem 1rem;
      font-size: 0.9rem;
    }

    .success-message {
      background: #d4edda;
      color: #155724;
      padding: 1rem;
      border-radius: 8px;
      margin-top: 1rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .error-message {
      background: #f8d7da;
      color: #721c24;
      padding: 1rem;
      border-radius: 8px;
      margin-top: 1rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    @media (max-width: 768px) {
      .form-row {
        grid-template-columns: 1fr;
      }
      
      .participants-input {
        flex-direction: column;
      }
      
      .form-actions {
        flex-direction: column;
      }
    }
  `]
})
export class CreateMeetingComponent {
  meeting: CreateMeetingDto = {
    title: '',
    description: '',
    startTime: new Date(),
    endTime: new Date(),
    participantEmails: []
  };

  startTimeString: string = '';
  endTimeString: string = '';
  newParticipant: string = '';
  isSubmitting: boolean = false;
  useJitsi: boolean = false;
  enableRecording: boolean = true;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private meetingService: MeetingService,
    private router: Router
  ) {
    // تعيين الوقت الافتراضي
    const now = new Date();
    const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
    this.meeting.startTime = tomorrow;
    this.meeting.endTime = new Date(tomorrow.getTime() + 60 * 60 * 1000); // ساعة واحدة
    
    this.startTimeString = this.formatDateTimeLocal(tomorrow);
    this.endTimeString = this.formatDateTimeLocal(new Date(tomorrow.getTime() + 60 * 60 * 1000));
  }

  addParticipant(): void {
    if (this.newParticipant && this.newParticipant.trim() && this.isValidEmail(this.newParticipant)) {
      if (!this.meeting.participantEmails.includes(this.newParticipant.trim())) {
        this.meeting.participantEmails.push(this.newParticipant.trim());
      }
      this.newParticipant = '';
    }
  }

  removeParticipant(index: number): void {
    this.meeting.participantEmails.splice(index, 1);
  }

  onSubmit(): void {
    if (this.isSubmitting) return;

    // تحويل النصوص إلى تواريخ
    this.meeting.startTime = new Date(this.startTimeString);
    this.meeting.endTime = new Date(this.endTimeString);

    // التحقق من صحة التواريخ
    if (this.meeting.startTime >= this.meeting.endTime) {
      this.errorMessage = 'يجب أن يكون وقت البداية قبل وقت النهاية';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    // فقط منطق محلي لإنشاء الميتنج
    this.meetingService.createMeeting(this.meeting).subscribe({
      next: (createdMeeting) => {
        this.isSubmitting = false;
        this.successMessage = 'تم إنشاء الميتنج بنجاح!';
        setTimeout(() => {
          this.router.navigate(['/meetings', createdMeeting.id]);
        }, 2000);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = 'حدث خطأ أثناء إنشاء الميتنج: ' + error.message;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/meetings']);
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private formatDateTimeLocal(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }
} 