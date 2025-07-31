import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CreateJitsiMeetingDto, JitsiMeeting } from '../../models/external-integration.model';

@Component({
  selector: 'app-jitsi-meeting',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './jitsi-meeting.component.html',
  styleUrls: ['./jitsi-meeting.component.scss']
})
export class JitsiMeetingComponent implements OnInit {
  meetingData: CreateJitsiMeetingDto = {
    roomName: '',
    startTime: new Date(),
    endTime: new Date(Date.now() + 60 * 60 * 1000), // 1 hour from now
    participantEmails: [],
    enableRecording: true
  };

  participantsText = '';
  currentMeeting: JitsiMeeting | null = null;
  isLoading = false;
  isRecordingLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    // Check if we have query parameters (coming from join meeting)
    this.route.queryParams.subscribe(params => {
      if (params['meetingId'] && params['roomName'] && params['jitsiUrl']) {
        // We have existing meeting data, load it
        this.loadExistingMeeting(params['meetingId']);
      } else {
        // Set default times for new meeting
        const now = new Date();
        this.meetingData.startTime = now;
        this.meetingData.endTime = new Date(now.getTime() + 60 * 60 * 1000); // 1 hour later
      }
    });
  }

  loadExistingMeeting(meetingId: string) {
    this.isLoading = true;
    this.errorMessage = '';

  }

  createMeeting() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Parse participants from text
    this.meetingData.participantEmails = this.participantsText
      .split(',')
      .map(email => email.trim())
      .filter(email => email.length > 0);

    // فقط منطق محلي أو استدعاء meetingService.createMeeting إذا كان موجود
    // مثال:
    // this.meetingService.createMeeting(this.meetingData).subscribe(...)
    // أو مجرد إعداد بيانات الاجتماع للعرض
    this.successMessage = 'تم تجهيز بيانات الميتنج!';
    this.isLoading = false;
  }

  startRecording() {
    if (!this.currentMeeting) return;

    this.isRecordingLoading = true;
    this.errorMessage = '';

  }

  stopRecording() {
    if (!this.currentMeeting) return;

    this.isRecordingLoading = true;
    this.errorMessage = '';

  }

  uploadRecording() {
    // This would typically involve file upload functionality
    this.successMessage = 'تم رفع التسجيل بنجاح!';
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
    
    // Clear query parameters
    this.router.navigate(['/jitsi-meeting']);
  }
} 