import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Input } from '@angular/core';
import { JitsiService } from '../../services/jitsi.service';
import { ActivatedRoute } from '@angular/router';
declare var JitsiMeetExternalAPI: any;

@Component({
  selector: 'app-embedded-jitsi',
  template: `<div #jitsiContainer class="jitsi-frame"></div>`,
  styleUrls: ['./embedded-jitsi.component.scss']
})
export class EmbeddedJitsiComponent implements OnInit, OnDestroy {
  @ViewChild('jitsiContainer', { static: true }) jitsiContainer!: ElementRef;
  @Input() roomName?: string;
  @Input() userName?: string;
  api: any;

  constructor(private jitsiService: JitsiService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.roomName = params['roomName'] || this.roomName;
      this.userName = params['userName'] || this.userName;
      if (!this.roomName || !this.userName) return;
      this.jitsiService.getJitsiToken(this.roomName, this.userName).subscribe(({ token }) => {
        console.log(token);
        this.api = new JitsiMeetExternalAPI('localhost:8443', {
          roomName: this.roomName,
          parentNode: this.jitsiContainer.nativeElement,
          userInfo: { displayName: this.userName },
          jwt: token,
        });
      });
    });
  }

  ngOnDestroy() {
    if (this.api) {
      this.api.dispose();
    }
  }
} 