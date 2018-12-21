import { Component, OnInit } from '@angular/core';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';
import { BankHolidayCalendar, BankHolidayCalendarYear, BankHolidayCalendarDate } from '.';
import { NzModalService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

export interface BankHolidayCalendarListItem extends BankHolidayCalendar {
	SelectedTabIndex: number;
}

export interface BankHolidayCalendarYearItem extends BankHolidayCalendarYear {
	AddingDate: boolean;
	YearDate: Date;
}

@Component({
	selector: 'bank-holiday-calendar',
	templateUrl: './bank-holiday-calendar.component.html',
	styleUrls: ['./bank-holiday-calendar.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarComponent implements OnInit {
	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	bankHolidayCalendarsList: BankHolidayCalendarListItem[] = [];

	isAddingNewCalendar: boolean = false;

	newCalendarName: string = '';
	newCalendarYear: Date;
	newCalendarYears: BankHolidayCalendarYearItem[] = [];
	nameAlreadyExisting: boolean = false;
	newDateForYear: Date;
	addingDate: boolean;

	isEdittingCalendar: boolean = false;
	edittingCalendar: BankHolidayCalendarListItem = null;
	newCalendarYearOfEditting: Date;

	newCalendarTabIndex: number;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.getBankHolidayCalendars().subscribe(calendars => {
			this.bankHolidayCalendarsList = calendars.map(c => {
				let item: BankHolidayCalendarListItem = {
					Id: c.Id,
					Name: c.Name,
					Years: c.Years,
					SelectedTabIndex: 0
				};
				return item;
			});
		});
	}

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

	deleteHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
		this.isEdittingCalendar = false;
	}

	editHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.edittingCalendar = calendar;
		this.isEdittingCalendar = true;
	}

	startAddNewBankCalender() {
		this.isAddingNewCalendar = true;
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

		this.newCalendarYears.forEach(y => (y.AddingDate = false));

		let newYear: BankHolidayCalendarYearItem = {
			Year: yearStr,
			YearDate: this.newDateForYear,
			AddingDate: true,
			Dates: []
		};
		this.newCalendarYears.push(newYear);
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
	}

	deleteYearTab(year: BankHolidayCalendarYearItem): void {
		this.newCalendarYears.splice(this.newCalendarYears.indexOf(year), 1);
	}

	addNewYearTabForExistingCalendar() {
		let yearStr = moment(this.newCalendarYearOfEditting).format(this.yearFormat);
		if (this.edittingCalendar.Years.some(y => y.Year == yearStr)) {
			return;
		}

		let newYear: BankHolidayCalendarYearItem = {
			Year: yearStr,
			AddingDate: false,
			YearDate: this.newCalendarYearOfEditting,
			Dates: [
				{
					Date: moment(yearStr).format(this.dateFormat),
					Description: this.translate.instant('BankHoliday')
				}
			]
		};

		this.edittingCalendar.Years.push(newYear);
	}

	startAddingDates(year: BankHolidayCalendarYearItem) {
		year.AddingDate = true;
	}

	addNewDateForYear(date: Date, year: BankHolidayCalendarYear) {
		let newDate: BankHolidayCalendarDate = {
			Date: moment(date).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday')
		};
		year.Dates = year.Dates.concat([newDate]);
	}

	removeDateOfYear(year: BankHolidayCalendarYear, date: BankHolidayCalendarDate) {
		year.Dates.splice(year.Dates.indexOf(date), 1);
	}

	disabledDate(date: Date): boolean {
		return (
			moment(date) < moment(this.newCalendarYear).startOf('year') ||
			moment(date) > moment(this.newCalendarYear).endOf('year')
		);
	}

	onChange(selectedDate: Date) {}

	cancelAddNewBankCalendar() {
		this.isAddingNewCalendar = false;
	}

	saveNewBankCalendar() {
		this.newCalendarYears.forEach(y => {
			let hash = {},
				dates = [];

			y.Dates.forEach(d => {
				d.Date = moment(d.Date).format(this.dateFormat);

				if (!hash[d.Date]) {
					hash[d.Date] = true;
					dates.push(d);
				}
			});
			y.Dates = dates.sort((c, n) => {
				return moment(c.Date) < moment(n.Date) ? -1 : 1;
			});
		});

		this.newCalendarYears.sort((c, n) => {
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.newCalendarYears
		};

		this.bankCalendarDataService.saveNewHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			let item: BankHolidayCalendarListItem = {
				Id: result.Id,
				Name: result.Name,
				Years: result.Years,
				SelectedTabIndex: 0
			};

			this.bankHolidayCalendarsList.push(item);
			this.isAddingNewCalendar = false;

			this.resetAddSpace();
		});
	}

	saveExistingHolidayCalendar(calendar: BankHolidayCalendarListItem) {
		calendar.Years.forEach(y => {
			let hash = {},
				dates = [];

			y.Dates.forEach(d => {
				d.Date = moment(d.Date).format(this.dateFormat);

				if (!hash[d.Date]) {
					hash[d.Date] = true;
					dates.push(d);
				}
			});
			y.Dates = dates.sort((c, n) => {
				return moment(c.Date) < moment(n.Date) ? -1 : 1;
			});
		});

		calendar.Years.sort((c, n) => {
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		this.bankCalendarDataService.saveNewHolidayCalendar(calendar).subscribe(result => {
			this.bankHolidayCalendarsList.forEach(c => {
				if (c.Id === result.Id) {
					c = result as BankHolidayCalendarListItem;
				}
			});
			this.isEdittingCalendar = false;
		});
	}

	resetAddSpace() {
		this.newCalendarName = null;
		this.newCalendarYear = null;
		this.newCalendarYears = [];
	}
}
