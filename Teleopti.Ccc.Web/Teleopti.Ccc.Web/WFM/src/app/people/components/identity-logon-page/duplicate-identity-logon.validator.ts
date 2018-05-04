import { Injectable } from '@angular/core';
import { ValidationErrors } from '@angular/forms';
import { Observable ,  of } from 'rxjs';
import { first, map, switchMap, take, tap } from 'rxjs/operators';
import { LogonInfoService } from '../../services';
import { FormControlWithInitial } from '../shared/forms';

@Injectable()
export class DuplicateIdentityLogonValidator {
	constructor(private logonInfoService: LogonInfoService) {}

	stateToErrorMessage = state => (state ? { duplicateNameValidator: state } : {});

	/**
	 * Override this to change the validation source
	 * @param value the value to check
	 */
	nameExists(value: string) {
		return this.logonInfoService.identityLogonExists(value);
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
