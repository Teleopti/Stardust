import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
	selector: 'app-feedback-message',
	templateUrl: './feedback-message.component.html',
	styleUrls: ['./feedback-message.component.scss']
})
export class FeedbackMessageComponent {
	@Input() name: string;
	@ViewChild('feedbackTemplate') feedbackTemplate;

	constructor(private translate: TranslateService) {}

	params = {};

	ngOnInit() {
		this.params = {
			'0': this.translate.instant(this.name),
			'1': '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">',
			'2': '</a>'
		};
	}

	getFeedbackMessage(): string {
		return this.translate.instant('WFMNewReleaseNotification', this.params);
	}
}
