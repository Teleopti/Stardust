import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl } from '@angular/forms';
import format from 'date-fns/format';
import { NzTreeNode } from 'ng-zorro-antd';
import { debounceTime } from 'rxjs/operators';
import { Moment } from '../../../../../node_modules/moment';
import { UserService } from '../../../core/services';
import { AuditTrailService } from '../../services';
import { Person, AuditEntry } from '../../../shared/types';

@Component({
	selector: 'general-audit-trail',
	templateUrl: './general-audit-trail.component.html',
	styleUrls: ['./general-audit-trail.component.scss'],
	providers: []
})
export class GeneralAuditTrailComponent implements OnInit {
	locale = 'en-US';

	person: Person;
	personList: Array<Person>;
	selectedPerson: Person;
	auditTrailData: Array<AuditEntry>;

	searchForm = this.fb.group({
		personPicker: [''],
		changedRange: [[new Date(), new Date()]]
	});

	constructor(
		private fb: FormBuilder,
		private auditTrailService: AuditTrailService,
		private userService: UserService
	) {
		this.userService.getPreferences().subscribe({
			next: prefs => {
				this.locale = prefs.DateFormatLocale;
			}
		});
	}

	ngOnInit() {
		this.auditTrailData = [];
		this.searchForm
			.get('personPicker')
			.valueChanges.pipe(debounceTime(700))
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
		this.auditTrailService
			.getPersonByKeyword(keyword)
			.subscribe(
				results => this.addPersonSearchToPersonList(results.Persons),
				error => this.personSearchError(error)
			);
	}

	personSearchError(error): string {
		return 'test';
	}

	addPersonSearchToPersonList(persons): void {
		this.personList = persons;
	}

	addAuditEntriesToTable(AuditEntries): void {
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
		if (this.searchForm.value.personPicker.Id) {
			return false;
		} else {
			return true;
		}
	}

	submitForm(): void {
		console.log(this.searchForm.value.changedRange[0]);
		this.auditTrailService
			.getStaffingAuditTrail(
				this.searchForm.value.personPicker.Id,
				moment(this.searchForm.value.changedRange[0]).format('YYYY-MM-DD'),
				moment(this.searchForm.value.changedRange[1]).format('YYYY-MM-DD')
			)
			.subscribe(results => this.addAuditEntriesToTable(results.AuditEntries));
	}
}
