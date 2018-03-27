import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormControl, AsyncValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { AppLogonService } from '../../services';
import { map, flatMap, filter, distinctUntilChanged, first } from 'rxjs/operators';
import { Observable } from 'rxjs';

export function duplicateNameValidator(appLogonService: AppLogonService): AsyncValidatorFn {
	const stateToErrorMessage = state => ({ duplicateNameValidator: state });

	return (control: AbstractControl): Observable<ValidationErrors> => {
		return Observable.timer(300).pipe(
			first(),
			filter(() => control.value != null && control.value.length > 0),
			flatMap(() => appLogonService.logonNameExists(control.value)),
			filter(exists => exists === true),
			map(exists => stateToErrorMessage(exists))
		);
	};
}

@Component({
	selector: 'people-form-field-input',
	templateUrl: './form-field-input.component.html',
	styleUrls: ['./form-field-input.component.scss']
})
export class FormFieldInputComponent implements OnInit {
	constructor(private appLogonService: AppLogonService) {}

	@Input() value: string;
	@Output() onValueChange = new EventEmitter<string>();

	formControl: FormControl;

	ngOnInit() {
		this.formControl = new FormControl(this.value);
		this.formControl.setAsyncValidators([duplicateNameValidator(this.appLogonService)]);
		this.formControl.valueChanges.pipe(distinctUntilChanged()).subscribe({
			next: value => {
				this.onValueChange.emit(value);
			}
		});
	}
}
