// newsletter.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export type EmailResponse = {
  email: string,
  recivedDate: string
}

@Injectable({ providedIn: 'root' })
export class DataService {
  private http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:5000/';

  // We return a Promise here to work easily with async/await in the component
  subscribeEmail(email: string): Promise<EmailResponse> {
    const route = 'email';
    return firstValueFrom(
      this.http.post<EmailResponse>(this.API_URL + route, { email })
    );
  }
}