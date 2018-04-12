import { Component, Injectable, OnDestroy, OnInit } from '@angular/core';
import {
	AsyncValidatorFn,
	FormArray,
	FormBuilder,
	FormControl,
	FormGroup,
	ValidationErrors,
	ValidatorFn
} from '@angular/forms';
import { AbstractControlOptions } from '@angular/forms/src/model';
import { Observable, Subject } from 'rxjs';
import { of } from 'rxjs/observable/of';
import { first, map, switchMap, take, tap } from 'rxjs/operators';
import { AppLogonService, NavigationService } from '../../services';
import { Person } from '../../types';
import { AppLogonPageService, PeopleWithAppLogon, PersonWithAppLogon } from './app-logon-page.service';

class FormControlWithInitial extends FormControl {
	constructor(
		formState?: any,
		validatorOrOpts?: ValidatorFn | ValidatorFn[] | AbstractControlOptions | null,
		asyncValidator?: AsyncValidatorFn | AsyncValidatorFn[] | null
	) {
		super(formState, validatorOrOpts, asyncValidator);
		this.initialValue = formState;
	}
	public initialValue: string;

	get sameAsInitialValue() {
		return this.initialValue === this.value.trim();
	}
}

@Injectable()
export class DuplicateNameValidator {
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

@Component({
	selector: 'people-app-logon-page',
	templateUrl: './app-logon-page.component.html',
	styleUrls: ['./app-logon-page.component.scss'],
	providers: [AppLogonPageService, DuplicateNameValidator]
})
export class AppLogonPageComponent implements OnDestroy, OnInit {
	constructor(
		public nav: NavigationService,
		private formBuilder: FormBuilder,
		private appLogonPageService: AppLogonPageService,
		private duplicateNameValidator: DuplicateNameValidator
	) {}

	private componentDestroyed: Subject<any> = new Subject();

	logonForm: FormGroup = this.formBuilder.group({
		logons: this.formBuilder.array([])
	});
	duplicationError: boolean = false;
	formValid: boolean = true;

	ngOnInit() {
		this.setupForm();
	}

	ngOnDestroy() {
		this.componentDestroyed.next();
		this.componentDestroyed.complete();
	}

	setupForm() {
		this.appLogonPageService.people$.takeUntil(this.componentDestroyed).subscribe({
			next: (people: PeopleWithAppLogon) => {
				if (people.length === 0) return this.nav.navToSearch();
				this.rebuildForm(people);
			}
		});
		this.logons.valueChanges.subscribe({
			next: value => this.checkForDuplicates(value)
		});
		this.logons.statusChanges.subscribe({
			next: status => {
				this.formValid = status === 'VALID';
			}
		});
	}

	rebuildForm(people: PeopleWithAppLogon) {
		while (this.logons.length > 0) this.logons.removeAt(0);
		people
			.map((person: PersonWithAppLogon) => {
				var formGroup = this.formBuilder.group({
					...person,
					ApplicationLogon: new FormControlWithInitial(person.ApplicationLogon)
				});
				formGroup.get('FullName').disable();
				var appLogon = formGroup.get('ApplicationLogon') as FormControl;
				appLogon.setAsyncValidators(this.duplicateNameValidator.validate);
				return formGroup;
			})
			.forEach(control => this.logons.push(control));
	}

	checkForDuplicates(people: Person[]) {
		const filterByAppLogon = ({ ApplicationLogon }: Person) => ApplicationLogon && ApplicationLogon.length > 0;
		let errors = people.filter(filterByAppLogon).reduce((errors, person, index, people) => {
			const filterBySameAppLogon = p => p.Id !== person.Id && p.ApplicationLogon === person.ApplicationLogon;
			const peopleWithSameLogon = people.filter(filterBySameAppLogon).length;
			if (peopleWithSameLogon === 0) return errors;
			return errors.concat({ [person.ApplicationLogon]: person.Id });
		}, []);
		this.duplicationError = errors.length > 0;
	}

	get logons(): FormArray {
		return this.logonForm.get('logons') as FormArray;
	}

	isValid(): boolean {
		return this.formValid && this.duplicationError === false;
	}

	save(): void {}
}
