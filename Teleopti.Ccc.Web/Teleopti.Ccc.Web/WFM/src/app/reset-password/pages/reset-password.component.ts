import { DOCUMENT } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { interval } from 'rxjs';
import { shareReplay, startWith, switchMap } from 'rxjs/operators';
import { ResetPasswordService } from '../shared/reset-password.service';

const getURLParameters = url =>
	(url.match(/([^?=&]+)(=([^&]*))/g) || []).reduce(
		(a, v) => ((a[v.slice(0, v.indexOf('='))] = v.slice(v.indexOf('=') + 1)), a),
		{}
	);

@Component({
	selector: 'reset-password',
	templateUrl: './reset-password.component.html',
	styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordPageComponent {
	constructor(@Inject(DOCUMENT) private document: Document, public resetPasswordService: ResetPasswordService) {
		this.token = getURLParameters(document.location.search).t;
	}

	token: string;
	successfulReset = false;
	hasValidToken$ = interval(15000).pipe(
		startWith(0),
		switchMap(() => {
			return this.resetPasswordService.isValidToken(this.token);
		}),
		shareReplay()
	);

	gotoLogin() {
		this.document.location.href = '../Authentication';
	}
}
