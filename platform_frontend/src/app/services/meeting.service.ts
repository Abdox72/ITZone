import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Meeting, CreateMeetingDto, UpdateMeetingDto, MeetingAnalysisResult } from '../models/meeting.model';

@Injectable({
  providedIn: 'root'
})
export class MeetingService {
  private apiUrl = 'https://localhost:7001/api/meetings'; // تأكد من تغيير المنفذ حسب إعداداتك

  constructor(private http: HttpClient) { }

  getAllMeetings(): Observable<Meeting[]> {
    return this.http.get<Meeting[]>(this.apiUrl);
  }

  getMeetingById(id: number): Observable<Meeting> {
    return this.http.get<Meeting>(`${this.apiUrl}/${id}`);
  }

  createMeeting(meeting: CreateMeetingDto): Observable<Meeting> {
    return this.http.post<Meeting>(this.apiUrl, meeting);
  }

  updateMeeting(id: number, meeting: UpdateMeetingDto): Observable<Meeting> {
    return this.http.put<Meeting>(`${this.apiUrl}/${id}`, meeting);
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
    return this.http.post<Meeting>(`${this.apiUrl}/${id}/complete`, {});
  }

  getUserMeetings(userId: string): Observable<Meeting[]> {
    return this.http.get<Meeting[]>(`${this.apiUrl}/user/${userId}`);
  }

  addParticipant(meetingId: number, email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${meetingId}/participants`, { email });
  }

  removeParticipant(meetingId: number, email: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${meetingId}/participants`, { body: { email } });
  }
} 