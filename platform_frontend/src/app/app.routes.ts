import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { MeetingListComponent } from './components/meeting-list/meeting-list.component';
import { CreateMeetingComponent } from './components/create-meeting/create-meeting.component';
import { MeetingDetailsComponent } from './components/meeting-details/meeting-details.component';
import { JitsiMeetingComponent } from './components/jitsi-meeting/jitsi-meeting.component';
import { EmbeddedJitsiComponent } from './components/embedded-jitsi/embedded-jitsi.component';
import { ExternalIntegrationsComponent } from './components/external-integrations/external-integrations.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { AuthGuard, GuestGuard } from './guards/auth.guard';

export const routes: Routes = [
  { 
    path: '', 
    component: HomeComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'meetings', 
    component: MeetingListComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'meetings/new', 
    component: CreateMeetingComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'meetings/:id', 
    component: MeetingDetailsComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'jitsi-meeting', 
    component: JitsiMeetingComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'embedded-jitsi', 
    component: EmbeddedJitsiComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'integrations', 
    component: ExternalIntegrationsComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'login', 
    component: LoginComponent,
    canActivate: [GuestGuard]
  },
  { 
    path: 'register', 
    component: RegisterComponent,
    canActivate: [GuestGuard]
  },
  { path: '**', redirectTo: '' }
];
