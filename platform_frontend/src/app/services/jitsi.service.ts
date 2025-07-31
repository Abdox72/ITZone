import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class JitsiService {
  constructor(private http: HttpClient) {}

  // جلب JWT من الباك اند
  getJitsiToken(room: string, user: string): Observable<{ token: string }> {
    //get backend url from environment
    const backendUrl = environment.apiUrl;
    return this.http.get<{ token: string }>(`${backendUrl}/jitsi/token?room=${room}&user=${user}`);
  }
} 