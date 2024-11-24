import { Component, signal } from '@angular/core';
import { JsonPipe } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HttpClientModule, JsonPipe, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  response = signal<SwitchDesktopResponse | undefined>(undefined);

  baseUrl = 'https://172.26.96.1:9999/';

  constructor(
    private readonly http: HttpClient,
  ) { }

  onSwitchToDefaultDesktop(): void {
    this.sendSwitchDesktop(DesktopType.default);
  }

  onSwitchToSecuredDesktop(): void {
    this.sendSwitchDesktop(DesktopType.secured);
  }

  sendSwitchDesktop(desktopType: DesktopType): void {
    const req: SwitchDesktopRequest = { desktopType };
    this.http.put<SwitchDesktopResponse>(this.getSwitchDesktopPath(), req).subscribe(value => {
      this.response.set(value);
    });
  }

  getSwitchDesktopPath(): string {
    return this.getPath('switch-desktop');
  }

  getPath(value: string): string {
    return `${this.baseUrl}${value}`;
  }
}

const enum DesktopType {
  default = 'default',
  secured = 'secured',
}

interface SwitchDesktopRequest {
  desktopType: DesktopType;
}


interface SwitchDesktopResponse {
  success: boolean;
}
