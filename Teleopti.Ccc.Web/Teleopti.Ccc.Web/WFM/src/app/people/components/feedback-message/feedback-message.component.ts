import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

@Component({
	selector: 'app-feedback-message',
	templateUrl: './feedback-message.component.html',
	styleUrls: ['./feedback-message.component.scss']
})
export class FeedbackMessageComponent {
	@Input() name: string;
	@ViewChild('feedbackTemplate') feedbackTemplate;

	constructor(private translate: TranslateService) {}

	ngOnInit() {}

	getFeedbackMessage(): Observable<string> {
		let params = {
			0: this.translate.instant(this.name),
			1: '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">',
			2: '</a>'
		};

		const translatedString = this.translate.get('WFMNewReleaseNotification', params);

		return translatedString;
	}
}
