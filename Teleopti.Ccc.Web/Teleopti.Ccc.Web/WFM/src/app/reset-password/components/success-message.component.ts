import { Component, EventEmitter, Output } from '@angular/core';

@Component({
	selector: 'success-message',
	template: `
	<nz-alert
		nzType="success"
		nzDescription="Your password is reset.">
	</nz-alert>
	<br>
	<button
		nz-button
		[nzType]="'primary'"
		(click)="linkAction.emit()">
		{{'LogOn' | translate}}
	</button>
	`
})
export class SuccessMessageComponent {
	@Output()
	linkAction = new EventEmitter();
}
