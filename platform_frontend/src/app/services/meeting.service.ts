import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Meeting, CreateMeetingDto, UpdateMeetingDto, MeetingAnalysisResult, MeetingStatus } from '../models/meeting.model';

// Backend response interfaces
interface BackendMeetingResponse {
  id: number;
  title: string;
  description: string;
  startTime: string;
  endTime: string;
  status: string;
  participantEmails: string[];
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private apiUrl = `${environment.apiUrl}/meeting`;

  constructor(private http: HttpClient) { }

  private mapBackendToFrontend(backendMeeting: BackendMeetingResponse): Meeting {
    return {
      id: backendMeeting.id,
      title: backendMeeting.title,
      description: backendMeeting.description,
      startTime: new Date(backendMeeting.startTime),
      endTime: new Date(backendMeeting.endTime),
      organizerName: 'المضيف', // Default value since backend doesn't provide this
      participantNames: backendMeeting.participantEmails,
      status: this.mapStatus(backendMeeting.status),
      summary: '', // Default empty since backend doesn't provide this
      tasks: [], // Default empty array since backend doesn't provide this
      createdAt: new Date(backendMeeting.createdAt)
    };
  }

  private mapStatus(backendStatus: string): MeetingStatus {
    switch (backendStatus.toLowerCase()) {
      case 'scheduled':
        return MeetingStatus.Scheduled;
      case 'inprogress':
        return MeetingStatus.InProgress;
      case 'completed':
        return MeetingStatus.Completed;
      case 'cancelled':
        return MeetingStatus.Cancelled;
      default:
        return MeetingStatus.Scheduled;
    }
  }

  getAllMeetings(): Observable<Meeting[]> {
    return this.http.get<BackendMeetingResponse[]>(this.apiUrl).pipe(
      map(meetings => meetings.map(meeting => this.mapBackendToFrontend(meeting)))
    );
  }

  getMeetingById(id: number): Observable<Meeting> {
    return this.http.get<BackendMeetingResponse>(`${this.apiUrl}/${id}`).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  createMeeting(meeting: CreateMeetingDto): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(this.apiUrl, meeting).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  updateMeeting(id: number, meeting: UpdateMeetingDto): Observable<Meeting> {
    return this.http.put<BackendMeetingResponse>(`${this.apiUrl}/${id}`, meeting).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  deleteMeeting(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  analyzeMeetingAudio(id: number, audioFile: File): Observable<MeetingAnalysisResult> {
    const formData = new FormData();
    formData.append('audioFile', audioFile);
    return this.http.post<MeetingAnalysisResult>(`${this.apiUrl}/${id}/analyze-audio`, formData);
  }

  completeMeeting(id: number): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/${id}/complete`, {}).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  getUserMeetings(userId: string): Observable<Meeting[]> {
    return this.http.get<BackendMeetingResponse[]>(`${this.apiUrl}/user/${userId}`).pipe(
      map(meetings => meetings.map(meeting => this.mapBackendToFrontend(meeting)))
    );
  }

  addParticipant(meetingId: number, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${meetingId}/participants`, { email });
  }

  removeParticipant(meetingId: number, email: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${meetingId}/participants`, { body: { email } });
  }
} 