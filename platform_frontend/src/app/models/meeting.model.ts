export interface Meeting {
  id: number;
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  organizerName: string;
  participantNames: string[];
  status: MeetingStatus;
  summary?: string;
  tasks: MeetingTask[];
  createdAt: Date;
}

export enum MeetingStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface MeetingTask {
  id: number;
  title: string;
  description: string;
  assignedToName: string;
  dueDate: Date;
  priority: TaskPriority;
  status: TaskStatus;
  externalPlatform: ExternalPlatform;
  externalTaskId?: string;
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum TaskStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export enum ExternalPlatform {
  None = 'None',
  Trello = 'Trello',
  Gmail = 'Gmail',
  GoogleCalendar = 'GoogleCalendar'
}

export interface CreateMeetingDto {
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  participantEmails: string[];
}

export interface UpdateMeetingDto {
  title: string;
  description: string;
  startTime: Date;
  endTime: Date;
  participantEmails: string[];
}

export interface MeetingAnalysisResult {
  summary: string;
  tasks: MeetingTask[];
  actionItems: string[];
  keyDecisions: string[];
  participantContributions: { [key: string]: string[] };
} 