import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AbstractControl } from '@angular/forms/src/model';
import { Subject } from 'rxjs';
import { NavigationService } from '../../services';
import { Person } from '../../types';
import { DuplicateIdentityLogonValidator, FormControlWithInitial } from '../shared';
import {
	IdentityLogonPageService,
	PersonWithIdentityLogon,
	PeopleWithIdentityLogon
} from './identity-logon-page.service';

class DuplicateFormNameValidator {
	private appLogonPageComponent: IdentityLogonPageComponent;

	constructor(appLogonPageComponent: IdentityLogonPageComponent) {
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
	selector: 'people-identity-logon-page',
	templateUrl: './identity-logon-page.component.html',
	styleUrls: ['./identity-logon-page.component.scss'],
	providers: [IdentityLogonPageService, DuplicateIdentityLogonValidator]
})
export class IdentityLogonPageComponent implements OnDestroy, OnInit {
	constructor(
		public nav: NavigationService,
		private formBuilder: FormBuilder,
		private identityLogonPageService: IdentityLogonPageService,
		private duplicateNameValidator: DuplicateIdentityLogonValidator
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
		this.identityLogonPageService.people$.takeUntil(this.componentDestroyed).subscribe({
			next: (people: PeopleWithIdentityLogon) => {
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

	rebuildForm(people: PeopleWithIdentityLogon) {
		while (this.logons.length > 0) this.logons.removeAt(0);
		people
			.map((person: PersonWithIdentityLogon) => {
				console.log(person)
				var formGroup = this.formBuilder.group({
					...person,
					IdentityLogon: new FormControlWithInitial(person.Identity)
				});
				formGroup.get('FullName').disable();
				var logon = formGroup.get('IdentityLogon') as FormControl;
				logon.setValidators([Validators.maxLength(50), new DuplicateFormNameValidator(this).validate]);
				logon.setAsyncValidators(this.duplicateNameValidator.validate);
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
		return this.logons.controls.map(control => control.get('IdentityLogon'));
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
		this.identityLogonPageService.save(people).subscribe({
			next: () => {
				this.nav.navToSearch();
			},
			error: err => {
				this.revalidateLogons();
			}
		});
	}
}
