import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting, MeetingStatus } from '../../models/meeting.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-meeting-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="meeting-list-container">
      <div class="header">
        <h1>الميتنجات</h1>
        <button class="btn btn-primary" routerLink="/meetings/new">
          <i class="fas fa-plus"></i>
          ميتنج جديد
        </button>
      </div>

      <div class="filters">
        <select [(ngModel)]="selectedStatus" (change)="filterMeetings()" class="form-select">
          <option value="">جميع الميتنجات</option>
          <option value="Scheduled">مجدولة</option>
          <option value="InProgress">قيد التنفيذ</option>
          <option value="Completed">مكتملة</option>
          <option value="Cancelled">ملغية</option>
        </select>
      </div>

      <div class="meetings-grid" *ngIf="filteredMeetings.length > 0; else noMeetings">
        <div class="meeting-card" *ngFor="let meeting of filteredMeetings" [routerLink]="['/meetings', meeting.id]">
          <div class="meeting-header">
            <h3>{{ meeting.title }}</h3>
            <span class="status-badge" [class]="getStatusClass(meeting.status)">
              {{ getStatusText(meeting.status) }}
            </span>
          </div>
          
          <p class="description">{{ meeting.description }}</p>
          
          <div class="meeting-details">
            <div class="detail">
              <i class="fas fa-calendar"></i>
              <span>{{ meeting.startTime | date:'short' }}</span>
            </div>
            <div class="detail">
              <i class="fas fa-user"></i>
              <span>{{ meeting.organizerName }}</span>
            </div>
            <div class="detail">
              <i class="fas fa-users"></i>
              <span>{{ meeting.participantNames.length }} مشارك</span>
            </div>
          </div>

          <div class="meeting-footer">
            <div class="tasks-count" *ngIf="meeting.tasks.length > 0">
              <i class="fas fa-tasks"></i>
              <span>{{ meeting.tasks.length }} مهمة</span>
            </div>
            <button class="btn btn-sm btn-outline-primary" (click)="$event.stopPropagation(); viewMeeting(meeting.id)">
              عرض التفاصيل
            </button>
          </div>
        </div>
      </div>

      <ng-template #noMeetings>
        <div class="no-meetings">
          <i class="fas fa-calendar-times"></i>
          <h3>لا توجد ميتنجات</h3>
          <p>ابدأ بإنشاء ميتنج جديد</p>
          <button class="btn btn-primary" routerLink="/meetings/new">
            إنشاء ميتنج
          </button>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .meeting-list-container {
      padding: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .header h1 {
      color: #2c3e50;
      margin: 0;
    }

    .filters {
      margin-bottom: 2rem;
    }

    .form-select {
      padding: 0.5rem;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 1rem;
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

    .description {
      color: #666;
      margin-bottom: 1rem;
      line-height: 1.5;
    }

    .meeting-details {
      margin-bottom: 1rem;
    }

    .detail {
      display: flex;
      align-items: center;
      margin-bottom: 0.5rem;
      color: #666;
    }

    .detail i {
      margin-left: 0.5rem;
      width: 16px;
    }

    .meeting-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 1rem;
      border-top: 1px solid #eee;
    }

    .tasks-count {
      display: flex;
      align-items: center;
      color: #666;
      font-size: 0.9rem;
    }

    .tasks-count i {
      margin-left: 0.25rem;
    }

    .no-meetings {
      text-align: center;
      padding: 4rem 2rem;
      color: #666;
    }

    .no-meetings i {
      font-size: 4rem;
      margin-bottom: 1rem;
      color: #ddd;
    }

    .no-meetings h3 {
      margin-bottom: 0.5rem;
      color: #2c3e50;
    }

    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.9rem;
      transition: all 0.2s;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:hover {
      background: #0056b3;
    }

    .btn-outline-primary {
      background: transparent;
      color: #007bff;
      border: 1px solid #007bff;
    }

    .btn-outline-primary:hover {
      background: #007bff;
      color: white;
    }

    .btn-sm {
      padding: 0.25rem 0.5rem;
      font-size: 0.8rem;
    }
  `]
})
export class MeetingListComponent implements OnInit {
  meetings: Meeting[] = [];
  filteredMeetings: Meeting[] = [];
  selectedStatus: string = '';

  constructor(private meetingService: MeetingService) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.meetingService.getAllMeetings().subscribe({
      next: (meetings) => {
        this.meetings = meetings;
        this.filteredMeetings = meetings;
      },
      error: (error) => {
        console.error('Error loading meetings:', error);
      }
    });
  }

  filterMeetings(): void {
    if (!this.selectedStatus) {
      this.filteredMeetings = this.meetings;
    } else {
      this.filteredMeetings = this.meetings.filter(
        meeting => meeting.status === this.selectedStatus as MeetingStatus
      );
    }
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

  viewMeeting(id: number): void {
    // سيتم التعامل مع هذا في Router
  }
} 