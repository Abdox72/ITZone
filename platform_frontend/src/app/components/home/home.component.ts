import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting, MeetingStatus } from '../../models/meeting.model';
import { JitsiMeeting } from '../../models/external-integration.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  recentMeetings: Meeting[] = [];
  stats = {
    totalMeetings: 0,
    totalTasks: 0,
    totalParticipants: 0,
    completionRate: 0
  };

  constructor(
    private meetingService: MeetingService,
    private router: Router
  ) {}

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

  joinMeeting(meeting: Meeting): void {
    const roomName = `meeting-${meeting.id}`;
    const userName = 'مستخدم'; // يمكنك استبداله باسم المستخدم من خدمة auth
    this.router.navigate(['/embedded-jitsi'], { queryParams: { roomName, userName } });
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