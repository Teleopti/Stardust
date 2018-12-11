import { Component, OnInit } from '@angular/core';
import { FormBuilder, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { UserService } from '../../../core/services';
import { AuditTrailService } from '../../services';
import { Person, AuditEntry } from '../../../shared/types';

class AuditTrailValidator {
	static INVALID_PERSON_ERROR = 'INVALID_PERSON_ERROR';
	static INVALID_DATE_RANGE_ERROR = 'INVALID_DATE_RANGE_ERROR';
	static validPerson: ValidatorFn = (control: AbstractControl): ValidationErrors => {
		const person: Person = control.value;
		if (person && person.Id) {
			return {};
		}
		return { [AuditTrailValidator.INVALID_PERSON_ERROR]: true };
	};

	static validateDateRange: ValidatorFn = (control: AbstractControl): ValidationErrors => {
		if (!control.value) {
			return {};
		}

		const [dateFrom, dateTo] = control.value;
		const numbersOfDaysBetween = moment(dateTo).diff(moment(dateFrom), 'days');

		if (numbersOfDaysBetween > 7) {
			return { [AuditTrailValidator.INVALID_DATE_RANGE_ERROR]: true };
		}
	};
}

@Component({
	selector: 'general-audit-trail',
	templateUrl: './general-audit-trail.component.html',
	styleUrls: ['./general-audit-trail.component.scss'],
	providers: []
})
export class GeneralAuditTrailComponent implements OnInit {
	locale = 'en-US';

	personList: Array<Person>;
	auditTrailData: Array<AuditEntry>;
	dateformat: Date;
	today = new Date();
	initialSearchDone = false;
	generalError = false;

	public get selectedPerson(): Person {
		return this.searchForm.value.personPicker;
	}

	public get personPickerControl(): AbstractControl {
		return this.searchForm.get('personPicker');
	}

	public get dateRangeControl(): AbstractControl {
		return this.searchForm.get('dateRange');
	}

	public get searchwordControl(): AbstractControl {
		return this.searchForm.get('searchword');
	}

	public get searchword(): string {
		return this.searchwordControl.value;
	}

	public get fromDate(): string {
		return moment(this.dateRangeControl.value[0]).format('YYYY-MM-DD');
	}

	public get toDate(): string {
		return moment(this.dateRangeControl.value[1]).format('YYYY-MM-DD');
	}

	searchForm = this.fb.group({
		personPicker: ['', [AuditTrailValidator.validPerson]],
		dateRange: [[new Date(), new Date()], [AuditTrailValidator.validateDateRange]],
		searchword: ['']
	});

	constructor(
		private fb: FormBuilder,
		private auditTrailService: AuditTrailService,
		private userService: UserService
	) {
		this.userService.preferences$.subscribe({
			next: prefs => {
				this.locale = prefs.DateFormatLocale;
			}
		});
	}

	ngOnInit() {
		this.auditTrailData = [];
		this.personPickerControl.valueChanges
			.pipe(debounceTime(700))
			.subscribe(value => this.updatePersonPicker(value));
	}

	updatePersonPicker(value): void {
		if (value.length > 1) {
			this.getPersonByKeyword(value);
		} else {
			this.personList = [];
		}
	}

	getPersonByKeyword(keyword): void {
		this.auditTrailService.getPersonByKeyword(keyword).subscribe(
			results => this.addPersonSearchToPersonList(results.Persons),
			error => {
				this.generalError = true;
			}
		);
	}

	addPersonSearchToPersonList(persons): void {
		this.personList = persons;
	}

	addAuditEntriesToTable(AuditEntries): void {
		this.initialSearchDone = true;
		var parsedEntries = AuditEntries.map(row => {
			return this.formatTimestampInRow(row);
		});

		this.auditTrailData = AuditEntries;
	}

	formatTimestampInRow(AuditEntry) {
		AuditEntry.TimeStamp = moment(AuditEntry.TimeStamp)
			.locale(this.locale)
			.format('L LTS');
		return AuditEntry;
	}

	notValidFields(): boolean {
		return (
			this.personPickerControl.hasError(AuditTrailValidator.INVALID_PERSON_ERROR) ||
			this.dateRangeControl.hasError(AuditTrailValidator.INVALID_DATE_RANGE_ERROR)
		);
	}

	submitForm(): void {
		this.generalError = false;
		this.auditTrailService
			.getStaffingAuditTrail(this.searchForm.value.personPicker.Id, this.fromDate, this.toDate, this.searchword)
			.subscribe(
				results => this.addAuditEntriesToTable(results.AuditEntries),
				error => {
					this.generalError = true;
				}
			);
	}
}
