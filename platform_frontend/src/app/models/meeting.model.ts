export enum MeetingStatus {
  Scheduled = 'scheduled',
  InProgress = 'in-progress',
  Completed = 'completed',
  Cancelled = 'cancelled'
}

export interface Meeting {
  id: number;
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  organizerName: string;
  participantNames: string[];
  status: MeetingStatus;
  summary: string;
  tasks: MeetingTask[];
  createdAt: Date;
  organizerEmail?: string;
  jitsiRoomName?: string;
  isRecordingEnabled?: boolean;
  recordingUrl?: string;
  meetingNotes?: string;
  startedAt?: Date;
  completedAt?: Date;
}

export interface MeetingTask {
  id: number;
  title: string;
  description: string;
  assignedTo: string;
  assignedToName?: string;
  priority: string;
  status: string;
  createdAt: Date;
  dueDate?: Date;
  externalPlatform?: string;
}

export interface MeetingAnalysisResult {
  summary: string;
  actionItems: string[];
  keyDecisions: string[];
  participants: string[];
  duration: number;
  transcript?: string;
  tasks?: MeetingTask[];
}

export interface CreateMeetingRequest {
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  participantEmails?: string[];
  organizerEmail?: string;
  isRecordingEnabled?: boolean;
}

export interface CreateMeetingDto {
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  participantEmails: string[];
  organizerEmail?: string;
  isRecordingEnabled?: boolean;
}

export interface UpdateMeetingRequest {
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  participantEmails?: string[];
  isRecordingEnabled?: boolean;
}

export interface AddTaskRequest {
  title: string;
  description: string;
  assignedTo: string;
  priority: string;
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  assignedTo: string;
  priority: string;
  status: string;
}

export interface UpdateNotesRequest {
  notes: string;
}

export interface UpdateRecordingRequest {
  recordingUrl: string;
} 