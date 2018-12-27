import { Component, OnInit } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import {
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarDateItem,
	BankHolidayCalendar
} from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-edit',
	templateUrl: './bank-holiday-calendar-edit.component.html',
	styleUrls: ['./bank-holiday-calendar-edit.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarEditComponent implements OnInit {
	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	bankHolidayCalendarsList: BankHolidayCalendar[];
	edittingCalendar: BankHolidayCalendar;
	editingCalendarName: string = '';
	editingCalendarYear: Date;
	editingCalendarYears: BankHolidayCalendarYearItem[] = [];
	nameAlreadyExisting: boolean = false;
	editingDateForYear: Date;

	editingCalendarTabIndex: number;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService
	) {}

	ngOnInit(): void {}

	confirmDeleteHolidayCanlendar(calendar: BankHolidayCalendar) {
		this.modalService.confirm({
			nzTitle: this.translate.instant('AreYouSureToDeleteThisBankHolidayCalendar'),
			nzContent: this.translate.instant('Name') + ': ' + calendar.Name,
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				this.deleteHolidayCanlendar(calendar);
			}
		});
	}

	checkNewCalendarName() {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(c => c.Name === this.editingCalendarName);
	}

	newYearTab(date: Date): void {
		this.editingDateForYear = new Date(
			moment(date)
				.startOf('year')
				.format(this.dateFormat)
		);
		let yearStr = moment(this.editingCalendarYear).format(this.yearFormat);
		if (this.editingCalendarYears.some(y => y.Year == yearStr)) {
			return;
		}

		this.editingCalendarYears.forEach(y => (y.Active = false));

		let newYear: BankHolidayCalendarYearItem = {
			Year: yearStr,
			YearDate: this.editingDateForYear,
			Active: true,
			Dates: [],
			SelectedDates: []
		};
		this.editingCalendarYears.push(newYear);
		this.editingCalendarTabIndex = this.editingCalendarYears.length - 1;
	}

	deleteYearTab(year: BankHolidayCalendarYearItem): void {
		this.editingCalendarYears.splice(this.editingCalendarYears.indexOf(year), 1);
	}

	addNewDateForYear(date: Date, year: BankHolidayCalendarYear) {
		let newDate: BankHolidayCalendarDateItem = {
			Date: moment(date).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
		};
		year.Dates = year.Dates.concat([newDate]);
	}

	removeDateOfYear(year: BankHolidayCalendarYear, date: BankHolidayCalendarDateItem) {
		year.Dates.splice(year.Dates.indexOf(date), 1);
	}

	disabledDate(date: Date): boolean {
		return (
			moment(date) < moment(this.editingCalendarYear).startOf('year') ||
			moment(date) > moment(this.editingCalendarYear).endOf('year')
		);
	}

	onChange(selectedDate: Date) {}

	saveNewBankCalendar() {
		this.editingCalendarYears.sort((c, n) => {
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		this.editingCalendarYears.forEach(y => {
			delete y.YearDate;
			delete y.DisabledDate;
			delete y.SelectedDates;
			delete y.Active;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.editingCalendarName,
			Years: this.editingCalendarYears as BankHolidayCalendarYear[]
		};

		this.bankCalendarDataService.saveExistingHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			let item: BankHolidayCalendar = {
				Id: result.Id,
				Name: result.Name,
				Years: result.Years
			};

			this.bankHolidayCalendarsList.push(item);

			this.resetAddSpace();
		});
	}

	resetAddSpace() {
		this.editingCalendarName = null;
		this.editingCalendarYear = null;
		this.editingCalendarYears = [];
	}

	deleteHolidayCanlendar(calendar: BankHolidayCalendar) {
		this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
	}
}
