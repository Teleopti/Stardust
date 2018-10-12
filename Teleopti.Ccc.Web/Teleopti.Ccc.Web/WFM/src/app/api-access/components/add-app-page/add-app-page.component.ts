import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { ExternalApplicationService, NavigationService } from '../../services';

@Component({
	selector: 'api-access-add-app-page',
	templateUrl: './add-app-page.component.html',
	styleUrls: ['./add-app-page.component.scss']
})
export class AddAppPageComponent implements OnInit {
	constructor(public nav: NavigationService, private externalApplicationService: ExternalApplicationService) {}
	form: FormGroup;

	ngOnInit() {
		this.form = new FormGroup({
			appFormControl: new FormControl('', [Validators.required, Validators.maxLength(50)]),
			tokenControl: new FormControl(''),
			token: new FormControl('')
		});
	}

	get token(): AbstractControl {
		return this.form.get('token');
	}

	get tokenValue(): string {
		return this.token.value;
	}

	get appFormControl(): AbstractControl {
		return this.form.get('appFormControl');
	}

	get appFormControlValue(): string {
		return this.appFormControl.value;
	}

	hasToken(): boolean {
		return this.tokenValue.length > 0;
	}

	save(): void {
		this.externalApplicationService.grantApiAccess(this.appFormControlValue).subscribe(token => {
			this.token.setValue(token.Token);
		});
	}
}
