import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AbstractControl } from '@angular/forms/src/model';
import { Subject } from 'rxjs';
import { NavigationService } from '../../services';
import { Person } from '../../types';
import { DuplicateAppLogonValidator, FormControlWithInitial } from '../shared';
import { AppLogonPageService, PeopleWithAppLogon, PersonWithAppLogon } from './app-logon-page.service';

class DuplicateFormNameValidator {
	private appLogonPageComponent: AppLogonPageComponent;

	constructor(appLogonPageComponent: AppLogonPageComponent) {
		this.appLogonPageComponent = appLogonPageComponent;
	}

	validate = (control: FormControlWithInitial): ValidationErrors => {
		const filterByExists = (appLogon: string) => appLogon && appLogon.length > 0;
		const filterBySameAppLogon = appLogon => appLogon === control.value;
		const countSameAppLogon = this.appLogonPageComponent.appLogons
			.map(control => control.value)
			.filter(filterByExists)
			.filter(filterBySameAppLogon).length;
		if (countSameAppLogon > 1) return { duplicateFormNameValidator: control.value };
		return {};
	};
}

@Component({
	selector: 'people-app-logon-page',
	templateUrl: './app-logon-page.component.html',
	styleUrls: ['./app-logon-page.component.scss'],
	providers: [AppLogonPageService, DuplicateAppLogonValidator]
})
export class AppLogonPageComponent implements OnDestroy, OnInit {
	constructor(
		public nav: NavigationService,
		private formBuilder: FormBuilder,
		private appLogonPageService: AppLogonPageService,
		private duplicateNameValidator: DuplicateAppLogonValidator
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
				appLogon.setValidators([Validators.maxLength(50), new DuplicateFormNameValidator(this).validate]);
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

	get appLogons(): AbstractControl[] {
		return this.logons.controls.map(control => control.get('ApplicationLogon'));
	}

	revalidateLogons() {
		this.appLogons.forEach(control => {
			control.updateValueAndValidity();
		});
	}

	isValid(): boolean {
		return this.formValid && this.duplicationError === false;
	}

	save(): void {
		const people = this.logonForm.get('logons').value;
		this.appLogonPageService.save(people).subscribe({
			next: () => {
				this.nav.navToSearch();
			},
			error: err => {
				this.revalidateLogons();
			}
		});
	}
}
