import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting, MeetingStatus } from '../../models/meeting.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="home-container">
      <!-- Hero Section -->
      <section class="hero-section">
        <div class="hero-content">
          <h1>منصة الميتنجات الذكية</h1>
          <p>قم بإدارة ميتنجاتك بذكاء واستخرج المهام تلقائياً من تسجيلات الصوت</p>
          <div class="hero-actions">
            <button class="btn btn-primary btn-large" routerLink="/meetings/new">
              <i class="fas fa-plus"></i>
              إنشاء ميتنج جديد
            </button>
            <button class="btn btn-outline btn-large" routerLink="/meetings">
              <i class="fas fa-calendar"></i>
              عرض الميتنجات
            </button>
          </div>
        </div>
        <div class="hero-image">
          <i class="fas fa-video"></i>
        </div>
      </section>

      <!-- Features Section -->
      <section class="features-section">
        <h2>المميزات الرئيسية</h2>
        <div class="features-grid">
          <div class="feature-card">
            <div class="feature-icon">
              <i class="fas fa-microphone"></i>
            </div>
            <h3>تحليل الصوت الذكي</h3>
            <p>قم برفع تسجيل الميتنج واحصل على نص مفصل وتحليل شامل للمحتوى</p>
          </div>
          <div class="feature-card">
            <div class="feature-icon">
              <i class="fas fa-tasks"></i>
            </div>
            <h3>استخراج المهام تلقائياً</h3>
            <p>يتم تحديد المهام والمسؤولين عنها تلقائياً من خلال تحليل المحادثة</p>
          </div>
          <div class="feature-card">
            <div class="feature-icon">
              <i class="fas fa-link"></i>
            </div>
            <h3>ربط مع المنصات الخارجية</h3>
            <p>ربط مباشر مع Trello و Gmail و Google Calendar لإدارة المهام</p>
          </div>
          <div class="feature-card">
            <div class="feature-icon">
              <i class="fas fa-chart-line"></i>
            </div>
            <h3>تقارير وتحليلات</h3>
            <p>احصل على تقارير مفصلة عن أداء الميتنجات والمهام المنجزة</p>
          </div>
        </div>
      </section>

      <!-- Recent Meetings Section -->
      <section class="recent-meetings-section" *ngIf="recentMeetings.length > 0">
        <div class="section-header">
          <h2>آخر الميتنجات</h2>
          <button class="btn btn-outline" routerLink="/meetings">
            عرض الكل
          </button>
        </div>
        <div class="meetings-grid">
          <div class="meeting-card" *ngFor="let meeting of recentMeetings" [routerLink]="['/meetings', meeting.id]">
            <div class="meeting-header">
              <h3>{{ meeting.title }}</h3>
              <span class="status-badge" [class]="getStatusClass(meeting.status)">
                {{ getStatusText(meeting.status) }}
              </span>
            </div>
            <p class="meeting-description">{{ meeting.description }}</p>
            <div class="meeting-meta">
              <span><i class="fas fa-calendar"></i> {{ meeting.startTime | date:'short' }}</span>
              <span><i class="fas fa-users"></i> {{ (meeting.participantNames?.length) || 0 }} مشارك</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Stats Section -->
      <section class="stats-section">
        <h2>إحصائيات سريعة</h2>
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-calendar-check"></i>
            </div>
            <div class="stat-content">
              <h3>{{ stats.totalMeetings }}</h3>
              <p>إجمالي الميتنجات</p>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-tasks"></i>
            </div>
            <div class="stat-content">
              <h3>{{ stats.totalTasks }}</h3>
              <p>المهام المحددة</p>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-users"></i>
            </div>
            <div class="stat-content">
              <h3>{{ stats.totalParticipants }}</h3>
              <p>المشاركون</p>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">
              <i class="fas fa-chart-line"></i>
            </div>
            <div class="stat-content">
              <h3>{{ stats.completionRate }}%</h3>
              <p>معدل الإنجاز</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .home-container {
      min-height: 100vh;
    }

    /* Hero Section */
    .hero-section {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      padding: 4rem 2rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 2rem;
    }

    .hero-content {
      flex: 1;
      max-width: 600px;
    }

    .hero-content h1 {
      font-size: 3rem;
      margin-bottom: 1rem;
      font-weight: 700;
    }

    .hero-content p {
      font-size: 1.2rem;
      margin-bottom: 2rem;
      opacity: 0.9;
      line-height: 1.6;
    }

    .hero-actions {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .hero-image {
      flex: 1;
      display: flex;
      justify-content: center;
      align-items: center;
    }

    .hero-image i {
      font-size: 8rem;
      opacity: 0.8;
    }

    /* Features Section */
    .features-section {
      padding: 4rem 2rem;
      background: #f8f9fa;
    }

    .features-section h2 {
      text-align: center;
      color: #2c3e50;
      margin-bottom: 3rem;
      font-size: 2.5rem;
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .feature-card {
      background: white;
      padding: 2rem;
      border-radius: 12px;
      text-align: center;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      transition: transform 0.2s;
    }

    .feature-card:hover {
      transform: translateY(-5px);
    }

    .feature-icon {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 1.5rem;
    }

    .feature-icon i {
      font-size: 2rem;
      color: white;
    }

    .feature-card h3 {
      color: #2c3e50;
      margin-bottom: 1rem;
      font-size: 1.5rem;
    }

    .feature-card p {
      color: #666;
      line-height: 1.6;
    }

    /* Recent Meetings Section */
    .recent-meetings-section {
      padding: 4rem 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .section-header h2 {
      color: #2c3e50;
      font-size: 2rem;
      margin: 0;
    }

    .meetings-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 1.5rem;
    }

    .meeting-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      transition: transform 0.2s, box-shadow 0.2s;
      cursor: pointer;
      border: 1px solid #e1e8ed;
    }

    .meeting-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 15px rgba(0, 0, 0, 0.15);
    }

    .meeting-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1rem;
    }

    .meeting-header h3 {
      margin: 0;
      color: #2c3e50;
      font-size: 1.2rem;
    }

    .status-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.8rem;
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

    .meeting-description {
      color: #666;
      margin-bottom: 1rem;
      line-height: 1.5;
    }

    .meeting-meta {
      display: flex;
      gap: 1rem;
      color: #666;
      font-size: 0.9rem;
    }

    .meeting-meta span {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    /* Stats Section */
    .stats-section {
      padding: 4rem 2rem;
      background: #f8f9fa;
    }

    .stats-section h2 {
      text-align: center;
      color: #2c3e50;
      margin-bottom: 3rem;
      font-size: 2.5rem;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .stat-card {
      background: white;
      padding: 2rem;
      border-radius: 12px;
      display: flex;
      align-items: center;
      gap: 1.5rem;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .stat-icon {
      width: 60px;
      height: 60px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .stat-icon i {
      font-size: 1.5rem;
      color: white;
    }

    .stat-content h3 {
      color: #2c3e50;
      font-size: 2rem;
      margin: 0 0 0.5rem 0;
    }

    .stat-content p {
      color: #666;
      margin: 0;
    }

    /* Buttons */
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

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
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

    .btn-large {
      padding: 1rem 2rem;
      font-size: 1.1rem;
    }

    @media (max-width: 768px) {
      .hero-section {
        flex-direction: column;
        text-align: center;
        padding: 2rem 1rem;
      }

      .hero-content h1 {
        font-size: 2rem;
      }

      .hero-actions {
        justify-content: center;
      }

      .hero-image i {
        font-size: 4rem;
      }

      .features-section, .recent-meetings-section, .stats-section {
        padding: 2rem 1rem;
      }

      .features-section h2, .stats-section h2 {
        font-size: 2rem;
      }

      .section-header {
        flex-direction: column;
        gap: 1rem;
        align-items: stretch;
      }

      .meetings-grid {
        grid-template-columns: 1fr;
      }

      .stats-grid {
        grid-template-columns: 1fr;
      }

      .stat-card {
        flex-direction: column;
        text-align: center;
      }
    }
  `]
})
export class HomeComponent implements OnInit {
  recentMeetings: Meeting[] = [];
  stats = {
    totalMeetings: 0,
    totalTasks: 0,
    totalParticipants: 0,
    completionRate: 0
  };

  constructor(private meetingService: MeetingService) {}

  ngOnInit(): void {
    this.loadRecentMeetings();
    this.loadStats();
  }

  loadRecentMeetings(): void {
    this.meetingService.getAllMeetings().subscribe({
      next: (meetings) => {
        this.recentMeetings = meetings.slice(0, 3); // آخر 3 ميتنجات
      },
      error: (error) => {
        console.error('Error loading recent meetings:', error);
      }
    });
  }

  loadStats(): void {
    this.meetingService.getAllMeetings().subscribe({
      next: (meetings) => {
        this.stats.totalMeetings = meetings.length;
        this.stats.totalTasks = meetings.reduce((sum, meeting) => sum + (meeting.tasks?.length || 0), 0);
        this.stats.totalParticipants = meetings.reduce((sum, meeting) => sum + (meeting.participantNames?.length || 0), 0);
        
        const completedMeetings = meetings.filter(m => m.status === MeetingStatus.Completed).length;
        this.stats.completionRate = meetings.length > 0 ? Math.round((completedMeetings / meetings.length) * 100) : 0;
      },
      error: (error) => {
        console.error('Error loading stats:', error);
      }
    });
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
} 