import { Component, OnInit } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import {
	BankHolidayCalendarListItem,
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

	edittingCalendar: BankHolidayCalendarListItem;

	newCalendarName: string = '';
	newCalendarYear: Date;
	newCalendarYears: BankHolidayCalendarYearItem[] = [];
	nameAlreadyExisting: boolean = false;
	newDateForYear: Date;
	addingDate: boolean;

	newCalendarTabIndex: number;
	bankHolidayCalendarsList: any;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService
	) {}

	ngOnInit(): void {}

	confirmDeleteHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
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
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(c => c.Name === this.newCalendarName);
	}

	newYearTab(date: Date): void {
		this.newDateForYear = new Date(
			moment(date)
				.startOf('year')
				.format(this.dateFormat)
		);
		let yearStr = moment(this.newCalendarYear).format(this.yearFormat);
		if (this.newCalendarYears.some(y => y.Year == yearStr)) {
			return;
		}

		this.newCalendarYears.forEach(y => (y.Active = false));

		let newYear: BankHolidayCalendarYearItem = {
			Year: yearStr,
			YearDate: this.newDateForYear,
			Active: true,
			Dates: [],
			SelectedDates: []
		};
		this.newCalendarYears.push(newYear);
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
	}

	deleteYearTab(year: BankHolidayCalendarYearItem): void {
		this.newCalendarYears.splice(this.newCalendarYears.indexOf(year), 1);
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
			moment(date) < moment(this.newCalendarYear).startOf('year') ||
			moment(date) > moment(this.newCalendarYear).endOf('year')
		);
	}

	onChange(selectedDate: Date) {}

	saveNewBankCalendar() {
		this.newCalendarYears.sort((c, n) => {
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		this.newCalendarYears.forEach(y => {
			delete y.YearDate;
			delete y.DisabledDate;
			delete y.SelectedDates;
			delete y.Active;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.newCalendarYears as BankHolidayCalendarYear[]
		};

		this.bankCalendarDataService.saveExistingHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			let item: BankHolidayCalendarListItem = {
				Id: result.Id,
				Name: result.Name,
				Years: result.Years,
				SelectedTabIndex: 0
			};

			this.bankHolidayCalendarsList.push(item);

			this.resetAddSpace();
		});
	}

	resetAddSpace() {
		this.newCalendarName = null;
		this.newCalendarYear = null;
		this.newCalendarYears = [];
	}

	deleteHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
	}
}
