import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MeetingService } from '../../services/meeting.service';
import { Meeting, MeetingStatus } from '../../models/meeting.model';
import { JitsiMeeting } from '../../models/external-integration.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-meeting-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './meeting-list.component.html',
  styleUrls: ['./meeting-list.component.scss']
})
export class MeetingListComponent implements OnInit {
  meetings: Meeting[] = [];
  filteredMeetings: Meeting[] = [];
  selectedStatus: string = '';

  constructor(
    private meetingService: MeetingService,
    private router: Router
  ) {}

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

  viewMeeting(id: number): void {
    this.router.navigate(['/meetings', id]);
  }
} 