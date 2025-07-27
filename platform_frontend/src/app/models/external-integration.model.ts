import { MeetingStatus, MeetingTask } from './meeting.model';

export interface JitsiMeeting {
  id: number;
  meetingId: string;
  roomName: string;
  jitsiUrl: string;
  startTime: Date;
  endTime: Date;
  status: MeetingStatus;
  isRecording: boolean;
  recordingUrl: string;
  participantEmails: string[];
  createdAt: Date;
}

export interface CreateJitsiMeetingDto {
  roomName: string;
  startTime: Date;
  endTime: Date;
  participantEmails: string[];
  enableRecording: boolean;
}

export interface ExternalIntegration {
  id: number;
  platform: string;
  isActive: boolean;
  tokenExpiry: Date;
  userEmail: string;
  connectedEmail?: string;
}

export interface ConnectExternalServiceDto {
  platform: string;
  authorizationCode: string;
}

export interface ExternalTaskCreationDto {
  meetingTaskId: number;
  platform: string;
  title: string;
  description: string;
  assignedToEmail: string;
  dueDate?: Date;
  priority: string;
}

export interface MeetingAnalysisEmailDto {
  meetingId: number;
  meetingTitle: string;
  summary: string;
  actionItems: string[];
  keyDecisions: string[];
  tasks: MeetingTask[];
  participantContributions: { [key: string]: string[] };
  recipientEmail: string;
}

export interface UploadRecordingDto {
  meetingId: number;
  recordingFile: File;
}

export interface RecordingResponse {
  recordingId: number;
  fileName: string;
  fileSize: number;
  message: string;
}

export interface TranscriptionResponse {
  transcription: string;
}

export interface ExternalTaskResponse {
  externalTaskId: number;
  externalUrl: string;
  message: string;
}

export interface EmailResponse {
  success: boolean;
  message: string;
}

export interface OAuthCallbackResponse {
  message: string;
  code: string;
  state: string;
} 