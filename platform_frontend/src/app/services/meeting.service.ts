import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  Meeting, 
  MeetingStatus, 
  CreateMeetingRequest, 
  UpdateMeetingRequest,
  AddTaskRequest,
  UpdateTaskRequest,
  UpdateNotesRequest,
  UpdateRecordingRequest
} from '../models/meeting.model';

interface BackendMeetingResponse {
  id: number;
  title: string;
  description: string;
  startTime: string;
  endTime: string;
  status: string;
  participantEmails: string[];
  createdAt: string;
  organizerEmail: string;
  jitsiRoomName: string;
  isRecordingEnabled: boolean;
  recordingUrl: string;
  meetingNotes: string;
  tasks: BackendTaskResponse[];
  startedAt?: string;
  completedAt?: string;
}

interface BackendTaskResponse {
  id: number;
  title: string;
  description: string;
  assignedTo: string;
  priority: string;
  status: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  private mapBackendToFrontend(backendMeeting: BackendMeetingResponse): Meeting {
    return {
      id: backendMeeting.id,
      title: backendMeeting.title,
      description: backendMeeting.description,
      startTime: new Date(backendMeeting.startTime),
      endTime: new Date(backendMeeting.endTime),
      organizerName: backendMeeting.organizerEmail, // Using organizer email as name
      participantNames: backendMeeting.participantEmails,
      status: this.mapStatus(backendMeeting.status),
      summary: backendMeeting.meetingNotes || '',
      tasks: backendMeeting.tasks.map(task => ({
        id: task.id,
        title: task.title,
        description: task.description,
        assignedTo: task.assignedTo,
        priority: task.priority,
        status: task.status,
        createdAt: new Date(task.createdAt)
      })),
      createdAt: new Date(backendMeeting.createdAt),
      organizerEmail: backendMeeting.organizerEmail,
      jitsiRoomName: backendMeeting.jitsiRoomName,
      isRecordingEnabled: backendMeeting.isRecordingEnabled,
      recordingUrl: backendMeeting.recordingUrl,
      meetingNotes: backendMeeting.meetingNotes,
      startedAt: backendMeeting.startedAt ? new Date(backendMeeting.startedAt) : undefined,
      completedAt: backendMeeting.completedAt ? new Date(backendMeeting.completedAt) : undefined
    };
  }

  private mapStatus(backendStatus: string): MeetingStatus {
    switch (backendStatus.toLowerCase()) {
      case 'scheduled':
        return MeetingStatus.Scheduled;
      case 'in-progress':
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
    return this.http.get<BackendMeetingResponse[]>(`${this.apiUrl}/meeting`).pipe(
      map(meetings => meetings.map(meeting => this.mapBackendToFrontend(meeting)))
    );
  }

  getMeetingById(id: number): Observable<Meeting> {
    return this.http.get<BackendMeetingResponse>(`${this.apiUrl}/meeting/${id}`).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  createMeeting(meetingData: CreateMeetingRequest): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting`, meetingData).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  updateMeeting(id: number, meetingData: UpdateMeetingRequest): Observable<Meeting> {
    return this.http.put<BackendMeetingResponse>(`${this.apiUrl}/meeting/${id}`, meetingData).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  deleteMeeting(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/meeting/${id}`);
  }

  completeMeeting(id: number): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting/${id}/complete`, {}).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  startMeeting(id: number): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting/${id}/start`, {}).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  getUserMeetings(userId: string): Observable<Meeting[]> {
    return this.http.get<BackendMeetingResponse[]>(`${this.apiUrl}/meeting/user/${userId}`).pipe(
      map(meetings => meetings.map(meeting => this.mapBackendToFrontend(meeting)))
    );
  }

  addParticipant(meetingId: number, email: string): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting/${meetingId}/participants`, { email }).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  removeParticipant(meetingId: number, email: string): Observable<Meeting> {
    return this.http.delete<BackendMeetingResponse>(`${this.apiUrl}/meeting/${meetingId}/participants`, { 
      body: { email } 
    }).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  addTask(meetingId: number, taskData: AddTaskRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/meeting/${meetingId}/tasks`, taskData);
  }

  updateTask(meetingId: number, taskId: number, taskData: UpdateTaskRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/meeting/${meetingId}/tasks/${taskId}`, taskData);
  }

  updateMeetingNotes(meetingId: number, notes: string): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting/${meetingId}/notes`, { notes }).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  updateRecordingUrl(meetingId: number, recordingUrl: string): Observable<Meeting> {
    return this.http.post<BackendMeetingResponse>(`${this.apiUrl}/meeting/${meetingId}/recording`, { recordingUrl }).pipe(
      map(meeting => this.mapBackendToFrontend(meeting))
    );
  }

  analyzeMeetingAudio(meetingId: number, audioFile: File): Observable<any> {
    const formData = new FormData();
    formData.append('audioFile', audioFile);
    return this.http.post(`${this.apiUrl}/meeting/${meetingId}/analyze-audio`, formData);
  }
} 