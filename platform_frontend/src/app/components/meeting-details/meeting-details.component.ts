import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting, MeetingStatus, MeetingAnalysisResult } from '../../models/meeting.model';

@Component({
  selector: 'app-meeting-details',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="meeting-details-container" *ngIf="meeting">
      <div class="header">
        <button class="btn btn-outline" (click)="goBack()">
          <i class="fas fa-arrow-right"></i>
          رجوع
        </button>
        <div class="actions">
          <button class="btn btn-outline" (click)="editMeeting()">
            <i class="fas fa-edit"></i>
            تعديل
          </button>
          <button class="btn btn-danger" (click)="deleteMeeting()">
            <i class="fas fa-trash"></i>
            حذف
          </button>
        </div>
      </div>

      <div class="meeting-content">
        <div class="meeting-header">
          <h1>{{ meeting.title }}</h1>
          <span class="status-badge" [class]="getStatusClass(meeting.status)">
            {{ getStatusText(meeting.status) }}
          </span>
        </div>

        <div class="meeting-info">
          <div class="info-section">
            <h3>معلومات الميتنج</h3>
            <div class="info-grid">
              <div class="info-item">
                <i class="fas fa-calendar"></i>
                <div>
                  <label>وقت البداية</label>
                  <span>{{ meeting.startTime | date:'full' }}</span>
                </div>
              </div>
              <div class="info-item">
                <i class="fas fa-clock"></i>
                <div>
                  <label>وقت النهاية</label>
                  <span>{{ meeting.endTime | date:'full' }}</span>
                </div>
              </div>
              <div class="info-item">
                <i class="fas fa-user"></i>
                <div>
                  <label>المنظم</label>
                  <span>{{ meeting.organizerName }}</span>
                </div>
              </div>
              <div class="info-item">
                <i class="fas fa-users"></i>
                <div>
                  <label>المشاركون</label>
                  <span>{{ meeting.participantNames.join(', ') }}</span>
                </div>
              </div>
            </div>
          </div>

          <div class="description-section" *ngIf="meeting.description">
            <h3>الوصف</h3>
            <p>{{ meeting.description }}</p>
          </div>

          <!-- قسم تحليل الصوت -->
          <div class="audio-analysis-section" *ngIf="meeting.status === MeetingStatus.Completed">
            <h3>تحليل الميتنج</h3>
            
            <div class="upload-audio" *ngIf="!analysisResult">
              <p>قم برفع تسجيل الميتنج لتحليله واستخراج المهام</p>
              <input
                type="file"
                #fileInput
                (change)="onFileSelected($event)"
                accept="audio/*"
                style="display: none;"
              />
              <button class="btn btn-primary" (click)="fileInput.click()">
                <i class="fas fa-upload"></i>
                رفع ملف صوتي
              </button>
              <button 
                class="btn btn-success" 
                (click)="analyzeAudio()"
                [disabled]="!selectedFile || isAnalyzing"
              >
                <i class="fas fa-magic"></i>
                {{ isAnalyzing ? 'جاري التحليل...' : 'تحليل الصوت' }}
              </button>
            </div>

            <div class="analysis-result" *ngIf="analysisResult">
              <div class="summary-section">
                <h4>ملخص الميتنج</h4>
                <p>{{ analysisResult.summary }}</p>
              </div>

              <div class="tasks-section">
                <h4>المهام المحددة</h4>
                <div class="tasks-list">
                  <div class="task-item" *ngFor="let task of analysisResult.tasks">
                    <div class="task-header">
                      <h5>{{ task.title }}</h5>
                      <span class="priority-badge" [class]="getPriorityClass(task.priority)">
                        {{ getPriorityText(task.priority) }}
                      </span>
                    </div>
                    <p>{{ task.description }}</p>
                    <div class="task-details">
                      <span><i class="fas fa-user"></i> {{ task.assignedToName }}</span>
                      <span><i class="fas fa-calendar"></i> {{ task.dueDate | date:'short' }}</span>
                      <span class="platform-badge" *ngIf="task.externalPlatform !== 'None'">
                        <i class="fas fa-link"></i> {{ getPlatformText(task.externalPlatform) }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="action-items-section" *ngIf="analysisResult.actionItems.length > 0">
                <h4>نقاط العمل</h4>
                <ul>
                  <li *ngFor="let item of analysisResult.actionItems">{{ item }}</li>
                </ul>
              </div>

              <div class="decisions-section" *ngIf="analysisResult.keyDecisions.length > 0">
                <h4>القرارات المهمة</h4>
                <ul>
                  <li *ngFor="let decision of analysisResult.keyDecisions">{{ decision }}</li>
                </ul>
              </div>
            </div>
          </div>

          <!-- قسم المهام الموجودة -->
          <div class="tasks-section" *ngIf="meeting.tasks.length > 0">
            <h3>المهام</h3>
            <div class="tasks-list">
              <div class="task-item" *ngFor="let task of meeting.tasks">
                <div class="task-header">
                  <h5>{{ task.title }}</h5>
                  <span class="status-badge" [class]="getTaskStatusClass(task.status)">
                    {{ getTaskStatusText(task.status) }}
                  </span>
                </div>
                <p>{{ task.description }}</p>
                <div class="task-details">
                  <span><i class="fas fa-user"></i> {{ task.assignedToName }}</span>
                  <span><i class="fas fa-calendar"></i> {{ task.dueDate | date:'short' }}</span>
                  <span class="priority-badge" [class]="getPriorityClass(task.priority)">
                    {{ getPriorityText(task.priority) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="loading" *ngIf="loading">
      <i class="fas fa-spinner fa-spin"></i>
      <p>جاري التحميل...</p>
    </div>
  `,
  styles: [`
    .meeting-details-container {
      padding: 2rem;
      max-width: 1000px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .actions {
      display: flex;
      gap: 1rem;
    }

    .meeting-content {
      background: white;
      border-radius: 12px;
      padding: 2rem;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .meeting-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 2rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid #eee;
    }

    .meeting-header h1 {
      color: #2c3e50;
      margin: 0;
    }

    .status-badge {
      padding: 0.5rem 1rem;
      border-radius: 20px;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .status-scheduled {
      background: #e3f2fd;
      color: #1976d2;
    }

    .status-inprogress {
      background: #fff3e0;
      color: #f57c00;
    }

    .status-completed {
      background: #e8f5e8;
      color: #388e3c;
    }

    .status-cancelled {
      background: #ffebee;
      color: #d32f2f;
    }

    .info-section, .description-section, .audio-analysis-section, .tasks-section {
      margin-bottom: 2rem;
    }

    .info-section h3, .description-section h3, .audio-analysis-section h3, .tasks-section h3 {
      color: #2c3e50;
      margin-bottom: 1rem;
    }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1rem;
    }

    .info-item {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .info-item i {
      color: #007bff;
      margin-top: 0.25rem;
    }

    .info-item div {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .info-item label {
      font-weight: 500;
      color: #666;
      font-size: 0.9rem;
    }

    .info-item span {
      color: #2c3e50;
    }

    .upload-audio {
      background: #f8f9fa;
      padding: 1.5rem;
      border-radius: 8px;
      text-align: center;
    }

    .upload-audio p {
      margin-bottom: 1rem;
      color: #666;
    }

    .upload-audio .btn {
      margin: 0 0.5rem;
    }

    .analysis-result {
      background: #f8f9fa;
      padding: 1.5rem;
      border-radius: 8px;
    }

    .summary-section, .tasks-section, .action-items-section, .decisions-section {
      margin-bottom: 1.5rem;
    }

    .summary-section h4, .tasks-section h4, .action-items-section h4, .decisions-section h4 {
      color: #2c3e50;
      margin-bottom: 0.5rem;
    }

    .tasks-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .task-item {
      background: white;
      padding: 1rem;
      border-radius: 8px;
      border: 1px solid #e1e8ed;
    }

    .task-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 0.5rem;
    }

    .task-header h5 {
      margin: 0;
      color: #2c3e50;
    }

    .task-details {
      display: flex;
      gap: 1rem;
      margin-top: 0.5rem;
      font-size: 0.9rem;
      color: #666;
    }

    .task-details span {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .priority-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 12px;
      font-size: 0.8rem;
      font-weight: 500;
    }

    .priority-low {
      background: #e8f5e8;
      color: #388e3c;
    }

    .priority-medium {
      background: #fff3e0;
      color: #f57c00;
    }

    .priority-high {
      background: #ffebee;
      color: #d32f2f;
    }

    .priority-critical {
      background: #fce4ec;
      color: #c2185b;
    }

    .platform-badge {
      background: #e3f2fd;
      color: #1976d2;
      padding: 0.25rem 0.5rem;
      border-radius: 12px;
      font-size: 0.8rem;
    }

    .action-items-section ul, .decisions-section ul {
      list-style: none;
      padding: 0;
    }

    .action-items-section li, .decisions-section li {
      padding: 0.5rem 0;
      border-bottom: 1px solid #eee;
    }

    .action-items-section li:last-child, .decisions-section li:last-child {
      border-bottom: none;
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

    .btn-success {
      background: #28a745;
      color: white;
    }

    .btn-success:hover:not(:disabled) {
      background: #218838;
    }

    .btn-outline {
      background: transparent;
      color: #666;
      border: 1px solid #ddd;
    }

    .btn-outline:hover {
      background: #f8f9fa;
    }

    .btn-danger {
      background: #dc3545;
      color: white;
    }

    .btn-danger:hover {
      background: #c82333;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem;
      color: #666;
    }

    .loading i {
      font-size: 2rem;
      margin-bottom: 1rem;
    }

    @media (max-width: 768px) {
      .header {
        flex-direction: column;
        gap: 1rem;
        align-items: stretch;
      }

      .actions {
        justify-content: center;
      }

      .meeting-header {
        flex-direction: column;
        gap: 1rem;
      }

      .info-grid {
        grid-template-columns: 1fr;
      }

      .task-details {
        flex-direction: column;
        gap: 0.5rem;
      }
    }
  `]
})
export class MeetingDetailsComponent implements OnInit {
  meeting: Meeting | null = null;
  loading: boolean = true;
  selectedFile: File | null = null;
  isAnalyzing: boolean = false;
  analysisResult: MeetingAnalysisResult | null = null;

  MeetingStatus = MeetingStatus; // للوصول إلى enum في template

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingService: MeetingService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadMeeting(+id);
    }
  }

  loadMeeting(id: number): void {
    this.meetingService.getMeetingById(id).subscribe({
      next: (meeting) => {
        this.meeting = meeting;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading meeting:', error);
        this.loading = false;
        alert('حدث خطأ أثناء تحميل الميتنج');
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.type.startsWith('audio/')) {
      this.selectedFile = file;
    } else {
      alert('يرجى اختيار ملف صوتي صحيح');
    }
  }

  analyzeAudio(): void {
    if (!this.selectedFile || !this.meeting) return;

    this.isAnalyzing = true;
    this.meetingService.analyzeMeetingAudio(this.meeting.id, this.selectedFile).subscribe({
      next: (result) => {
        this.analysisResult = result;
        this.isAnalyzing = false;
        alert('تم تحليل الميتنج بنجاح!');
        // إعادة تحميل الميتنج لعرض المهام الجديدة
        this.loadMeeting(this.meeting!.id);
      },
      error: (error) => {
        this.isAnalyzing = false;
        console.error('Error analyzing audio:', error);
        alert('حدث خطأ أثناء تحليل الصوت');
      }
    });
  }

  editMeeting(): void {
    if (this.meeting) {
      this.router.navigate(['/meetings', this.meeting.id, 'edit']);
    }
  }

  deleteMeeting(): void {
    if (!this.meeting) return;

    if (confirm('هل أنت متأكد من حذف هذا الميتنج؟')) {
      this.meetingService.deleteMeeting(this.meeting.id).subscribe({
        next: () => {
          alert('تم حذف الميتنج بنجاح');
          this.router.navigate(['/meetings']);
        },
        error: (error) => {
          console.error('Error deleting meeting:', error);
          alert('حدث خطأ أثناء حذف الميتنج');
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/meetings']);
  }

  getStatusClass(status: MeetingStatus): string {
    switch (status) {
      case MeetingStatus.Scheduled:
        return 'status-scheduled';
      case MeetingStatus.InProgress:
        return 'status-inprogress';
      case MeetingStatus.Completed:
        return 'status-completed';
      case MeetingStatus.Cancelled:
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getStatusText(status: MeetingStatus): string {
    switch (status) {
      case MeetingStatus.Scheduled:
        return 'مجدولة';
      case MeetingStatus.InProgress:
        return 'قيد التنفيذ';
      case MeetingStatus.Completed:
        return 'مكتملة';
      case MeetingStatus.Cancelled:
        return 'ملغية';
      default:
        return status;
    }
  }

  getPriorityClass(priority: string): string {
    switch (priority) {
      case 'Low':
        return 'priority-low';
      case 'Medium':
        return 'priority-medium';
      case 'High':
        return 'priority-high';
      case 'Critical':
        return 'priority-critical';
      default:
        return '';
    }
  }

  getPriorityText(priority: string): string {
    switch (priority) {
      case 'Low':
        return 'منخفضة';
      case 'Medium':
        return 'متوسطة';
      case 'High':
        return 'عالية';
      case 'Critical':
        return 'حرجة';
      default:
        return priority;
    }
  }

  getTaskStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'status-scheduled';
      case 'InProgress':
        return 'status-inprogress';
      case 'Completed':
        return 'status-completed';
      case 'Cancelled':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  getTaskStatusText(status: string): string {
    switch (status) {
      case 'Pending':
        return 'في الانتظار';
      case 'InProgress':
        return 'قيد التنفيذ';
      case 'Completed':
        return 'مكتملة';
      case 'Cancelled':
        return 'ملغية';
      default:
        return status;
    }
  }

  getPlatformText(platform: string): string {
    switch (platform) {
      case 'Trello':
        return 'Trello';
      case 'Gmail':
        return 'Gmail';
      case 'GoogleCalendar':
        return 'Google Calendar';
      default:
        return platform;
    }
  }
} 