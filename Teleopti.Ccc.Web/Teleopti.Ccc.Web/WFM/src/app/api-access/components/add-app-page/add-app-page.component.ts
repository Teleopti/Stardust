import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, ValidationErrors, Validators, FormGroup } from '@angular/forms';
import { AbstractControl } from '@angular/forms/src/model';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { NavigationService } from '../../services';
import { FormControlWithInitial } from '../shared';
import { AddAppPageService } from './add-app-page.service';

@Component({
	selector: 'api-access-add-app-page',
	templateUrl: './add-app-page.component.html',
	styleUrls: ['./add-app-page.component.scss'],
	providers: [AddAppPageService, NavigationService]
})
export class AddAppPageComponent implements OnInit {
	constructor(
		public nav: NavigationService,
		private addAppPageService: AddAppPageService
	) {}

	token = '';
	waitingForToken = false;

	appFormControl = new FormControl('', [
		Validators.required,
		Validators.maxLength(50)
	]);

	tokenControl = new FormControl('');

	form = new FormGroup({
		appFormControl: this.appFormControl,
		tokenControl: this.tokenControl
	});

	ngOnInit() {
	}

	hasToken(): boolean {
		return this.token.length > 0;
	}
	
	save(): void {
		this.waitingForToken = true;
		this.addAppPageService.save(this.appFormControl.value).subscribe(token => {
			this.token = token.Token;
			this.tokenControl.setValue(this.token);
			this.waitingForToken = false;
		});
	}
}
