import { Component, Input, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

@Component({
	selector: 'feedback-message',
	templateUrl: './feedback-message.component.html',
	styleUrls: ['./feedback-message.component.scss']
})
export class FeedbackMessageComponent {
	@Input()
	componentName: string;
	@Input()
	feedbackType: string;
	@ViewChild('feedbackTemplate')
	feedbackTemplate;

	constructor(private translateService: TranslateService) {}

	getFeedbackMessage(): Observable<string> {
		if (this.feedbackType === 'newRelease') {
			return this.translateService.get('WFMNewReleaseNotification', {
				0: this.componentName,
				1: '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">',
				2: '</a>'
			});
		} else if (this.feedbackType === 'updated') {
			return this.translateService.get('WFMReleaseNotificationWithoutOldModuleLink', {
				0: this.componentName,
				1: '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">',
				2: '</a>'
			});
		}
	}
}
