import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AbstractControl } from '@angular/forms/src/model';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { FormControlWithInitial, NavigationService } from '../../shared';
import { AppLogonPageService, PeopleWithLogon, PersonWithLogon } from './app-logon-page.service';
import { DuplicateAppLogonValidator } from './duplicate-app-logon.validator';

class DuplicateFormNameValidator {
	private appLogonPageComponent: AppLogonPageComponent;

	constructor(appLogonPageComponent: AppLogonPageComponent) {
		this.appLogonPageComponent = appLogonPageComponent;
	}

	validate = (control: FormControlWithInitial): ValidationErrors => {
		const filterByExists = (logon: string) => logon && logon.length > 0;
		const filterBySameLogon = (logon: string) => logon.toLowerCase() === (control.value as string).toLowerCase();
		const countSameLogon = this.appLogonPageComponent.logons
			.map(c => c.value)
			.filter(filterByExists)
			.filter(filterBySameLogon).length;
		if (countSameLogon > 1) return { duplicateFormNameValidator: control.value };
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

	form: FormGroup = this.formBuilder.group({
		people: this.formBuilder.array([])
	});

	ngOnInit() {
		this.appLogonPageService.people$.pipe(takeUntil(this.componentDestroyed)).subscribe({
			next: (people: PeopleWithLogon) => {
				if (people.length === 0) return this.nav.navToSearch();
				this.buildForm(people);
			}
		});
		this.people.valueChanges.pipe(debounceTime(100)).subscribe({
			next: val => {
				this.logons
					.filter(logon => logon.invalid)
					.forEach(control => {
						control.updateValueAndValidity();
					});
			}
		});
	}

	ngOnDestroy() {
		this.componentDestroyed.next();
		this.componentDestroyed.complete();
	}

	personToFormGroup(person: PersonWithLogon) {
		const Id = new FormControl(person.Id);
		Id.disable();
		const FullName = new FormControl(person.FullName);
		FullName.disable();
		const Logon = new FormControlWithInitial(person.Logon);
		Logon.setValidators([Validators.maxLength(50), new DuplicateFormNameValidator(this).validate]);
		Logon.setAsyncValidators(this.duplicateNameValidator.validate);

		return this.formBuilder.group({
			Id,
			FullName,
			Logon
		});
	}

	buildForm(people: PeopleWithLogon) {
		while (this.people.length > 0) this.people.removeAt(0);
		people.map(p => this.personToFormGroup(p)).forEach(control => this.people.push(control));
	}

	get people(): FormArray {
		return this.form.get('people') as FormArray;
	}

	get logons(): AbstractControl[] {
		return this.people.controls.map(control => control.get('Logon'));
	}

	revalidateLogons() {
		this.logons.forEach(control => {
			control.updateValueAndValidity();
		});
	}

	isValid(): boolean {
		return this.form.valid;
	}

	save(): void {
		const people = this.people.getRawValue();
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
