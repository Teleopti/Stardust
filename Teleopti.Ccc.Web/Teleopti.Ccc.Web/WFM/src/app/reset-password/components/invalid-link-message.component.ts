import { Component } from '@angular/core';

@Component({
	selector: 'invalid-link-message',
	template: `
	<nz-alert
		nzType="error"
		nzMessage="Invalid link"
		nzDescription="Your link has expired or is invalid.">
	</nz-alert>
	`
})
export class InvalidLinkMessageComponent {}
