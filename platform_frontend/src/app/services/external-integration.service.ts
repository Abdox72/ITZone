import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  JitsiMeeting,
  CreateJitsiMeetingDto,
  ExternalIntegration,
  ConnectExternalServiceDto,
  ExternalTaskCreationDto,
  MeetingAnalysisEmailDto,
  UploadRecordingDto,
  RecordingResponse,
  TranscriptionResponse,
  ExternalTaskResponse,
  EmailResponse,
  OAuthCallbackResponse
} from '../models/external-integration.model';

@Injectable({
  providedIn: 'root'
})
export class ExternalIntegrationService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Jitsi Meeting Methods
  createJitsiMeeting(createDto: CreateJitsiMeetingDto): Observable<JitsiMeeting> {
    return this.http.post<JitsiMeeting>(`${this.apiUrl}/external-integration/jitsi/create`, createDto);
  }

  getJitsiMeeting(meetingId: number): Observable<JitsiMeeting> {
    return this.http.get<JitsiMeeting>(`${this.apiUrl}/external-integration/jitsi/${meetingId}`);
  }

  startRecording(meetingId: number): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/external-integration/jitsi/${meetingId}/start-recording`, {});
  }

  stopRecording(meetingId: number): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/external-integration/jitsi/${meetingId}/stop-recording`, {});
  }

  getRecordingUrl(meetingId: number): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/external-integration/jitsi/${meetingId}/recording-url`);
  }

  // External Service Integration Methods
  getAuthorizationUrl(platform: string): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/external-integration/auth-url/${platform}`);
  }

  connectExternalService(connectDto: ConnectExternalServiceDto): Observable<ExternalIntegration> {
    return this.http.post<ExternalIntegration>(`${this.apiUrl}/external-integration/connect`, connectDto);
  }

  getUserIntegrations(): Observable<ExternalIntegration[]> {
    return this.http.get<ExternalIntegration[]>(`${this.apiUrl}/external-integration/integrations`);
  }

  disconnectExternalService(integrationId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/external-integration/integrations/${integrationId}`);
  }

  // Recording Management Methods
  uploadRecording(uploadDto: UploadRecordingDto): Observable<RecordingResponse> {
    const formData = new FormData();
    formData.append('meetingId', uploadDto.meetingId.toString());
    formData.append('recordingFile', uploadDto.recordingFile);
    
    return this.http.post<RecordingResponse>(`${this.apiUrl}/external-integration/recordings/upload`, formData);
  }

  transcribeRecording(recordingId: number): Observable<TranscriptionResponse> {
    return this.http.post<TranscriptionResponse>(`${this.apiUrl}/external-integration/recordings/${recordingId}/transcribe`, {});
  }

  analyzeTranscription(transcription: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/external-integration/recordings/analyze`, { transcription });
  }

  // Task Creation in External Platforms
  createTaskInTrello(taskDto: ExternalTaskCreationDto): Observable<ExternalTaskResponse> {
    return this.http.post<ExternalTaskResponse>(`${this.apiUrl}/external-integration/tasks/trello`, taskDto);
  }

  createTaskInGmail(taskDto: ExternalTaskCreationDto): Observable<ExternalTaskResponse> {
    return this.http.post<ExternalTaskResponse>(`${this.apiUrl}/external-integration/tasks/gmail`, taskDto);
  }

  createTaskInGoogleCalendar(taskDto: ExternalTaskCreationDto): Observable<ExternalTaskResponse> {
    return this.http.post<ExternalTaskResponse>(`${this.apiUrl}/external-integration/tasks/google-calendar`, taskDto);
  }

  // Email Integration Methods
  sendMeetingAnalysisEmail(emailDto: MeetingAnalysisEmailDto): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/external-integration/email/send-analysis`, emailDto);
  }

  sendTaskNotificationEmail(task: any, recipientEmail: string): Observable<EmailResponse> {
    return this.http.post<EmailResponse>(`${this.apiUrl}/external-integration/email/send-task-notification`, { task, recipientEmail });
  }

  // OAuth Callback
  oauthCallback(code: string, state: string): Observable<OAuthCallbackResponse> {
    return this.http.get<OAuthCallbackResponse>(`${this.apiUrl}/external-integration/callback?code=${code}&state=${state}`);
  }

  // Automated Processing
  processRecordingAutomatically(recordingId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/external-integration/recordings/${recordingId}/process`, {});
  }
} 