import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl } from '@angular/forms';
import format from 'date-fns/format';
import { NzTreeNode } from 'ng-zorro-antd';
import { debounceTime } from 'rxjs/operators';
import { Moment } from '../../../../../node_modules/moment';
import { UserService } from '../../../core/services';
import { AuditTrailService } from '../../services';
import { Person } from '../../../shared/types';

const mapToISODate = date => format(date, 'YYYY-MM-DD');
const toISORange = ([startDate, endDate]: DateRange): DateRange => [mapToISODate(startDate), mapToISODate(endDate)];

interface DateRange extends Array<string> {
	[0]: string;
	[1]: string;
}

@Component({
	selector: 'general-audit-trail',
	templateUrl: './general-audit-trail.component.html',
	styleUrls: ['./general-audit-trail.component.scss'],
	providers: []
})
export class GeneralAuditTrailComponent implements OnInit {
	dateformat = 'YYYY-MM-DD';
	moment: Moment;

	person: Person;
	personList: Array<Person>;
	selectedPerson: Person;

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
				this.moment = moment().locale(prefs.DateFormatLocale);
			}
		});
	}

	ngOnInit() {
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

	submitForm() {
		console.log(this.searchForm.value);
	}
}
