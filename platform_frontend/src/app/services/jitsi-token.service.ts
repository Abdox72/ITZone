// src/app/services/jitsi-token.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class JitsiTokenService {
  constructor(private http: HttpClient) {}

  getToken(room: string, user: string) {
    // adjust port if needed
    return this.http.get<{ token: string }>(
      `/api/jitsi/token?room=${room}&user=${user}`
    );
  }
}