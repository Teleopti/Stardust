import { Injectable } from '@angular/core';
import { ValidationErrors } from '@angular/forms';
import { Observable } from 'rxjs';
import { of } from 'rxjs/observable/of';
import { first, map, switchMap, take, tap } from 'rxjs/operators';
import { AppLogonService } from '../../../services';
import { FormControlWithInitial } from '../forms';

@Injectable()
export class DuplicateAppLogonValidator {
	constructor(private appLogonService: AppLogonService) {}

	stateToErrorMessage = state => (state ? { duplicateNameValidator: state } : {});

	/**
	 * Override this to change the validation source
	 * @param value the value to check
	 */
	nameExists(value: string) {
		return this.appLogonService.logonNameExists(value);
	}

	validate = (control: FormControlWithInitial): Observable<ValidationErrors> => {
		if (control.sameAsInitialValue) return of(this.stateToErrorMessage(false));

		return Observable.timer(500).pipe(
			switchMap(() => this.nameExists(control.value)),
			take(1),
			map(isDuplicate => this.stateToErrorMessage(isDuplicate)),
			tap(() => control.markAsTouched()),
			first()
		);
	};
}
