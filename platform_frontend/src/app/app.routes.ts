import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { MeetingListComponent } from './components/meeting-list/meeting-list.component';
import { CreateMeetingComponent } from './components/create-meeting/create-meeting.component';
import { MeetingDetailsComponent } from './components/meeting-details/meeting-details.component';
import { JitsiMeetingComponent } from './components/jitsi-meeting/jitsi-meeting.component';
import { ExternalIntegrationsComponent } from './components/external-integrations/external-integrations.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'meetings', component: MeetingListComponent },
  { path: 'meetings/new', component: CreateMeetingComponent },
  { path: 'meetings/:id', component: MeetingDetailsComponent },
  { path: 'jitsi', component: JitsiMeetingComponent },
  { path: 'integrations', component: ExternalIntegrationsComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: '**', redirectTo: '' }
];
