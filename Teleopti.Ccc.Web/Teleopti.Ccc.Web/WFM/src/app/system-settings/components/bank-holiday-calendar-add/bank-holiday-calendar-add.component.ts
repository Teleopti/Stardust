import { Component, OnInit, Input, ChangeDetectorRef } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import {
	BankHolidayCalendar,
	BankHolidayCalendarYear,
	BankHolidayCalendarDate,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarListItem
} from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-add',
	templateUrl: './bank-holiday-calendar-add.component.html',
	styleUrls: ['./bank-holiday-calendar-add.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarAddComponent implements OnInit {
	@Input() bankHolidayCalendarsList: BankHolidayCalendarListItem[];
	@Input() cancelAddNewBankCalendar: Function;

	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	newCalendarName: string = '';
	newCalendarYear: Date;
	newCalendarYears: BankHolidayCalendarYearItem[] = [];
	nameAlreadyExisting: boolean = false;
	newDateForYear: Date;
	newCalendarTabIndex: number;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private changeDetectorRef: ChangeDetectorRef
	) {}

	ngOnInit(): void {}

	ngAfterViewInit(): void {
		this.changeDetectorRef.detectChanges();
	}

	checkNewCalendarName() {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(c => c.Name === this.newCalendarName);
	}

	newYearTab(date: Date): void {
		let newCalendarYearDate = new Date(
			moment(date)
				.startOf('year')
				.format(this.dateFormat)
		);
		let yearStr = moment(newCalendarYearDate).format(this.yearFormat);
		if (this.newCalendarYears.some(y => y.Year == yearStr)) {
			return;
		}

		this.newCalendarYears.forEach(y => (y.Active = false));

		let newYear = {
			Year: yearStr,
			YearDate: newCalendarYearDate,
			DisabledDate: date => {
				return moment(date) < moment(yearStr).startOf('year') || moment(date) > moment(yearStr).endOf('year');
			},
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

	formatDate(date: Date): string {
		return moment(date).format(this.dateFormat);
	}

	addNewDateForYear(date: Date, year: BankHolidayCalendarYearItem) {
		if (year.SelectedDates.indexOf(date.getTime()) > -1) return;

		let newDate: BankHolidayCalendarDate = {
			Date: moment(date).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday')
		};

		year.Dates.push(newDate);
		year.Dates.sort((c, n) => {
			return moment(c.Date) < moment(n.Date) ? -1 : 1;
		});
		year.Dates = [...year.Dates];

		year.SelectedDates.push(date.getTime());
		year.SelectedDates.sort((c, n) => {
			return moment(c) < moment(n) ? -1 : 1;
		});
		year.SelectedDates = [...year.SelectedDates];
	}

	removeDateOfYear(year: BankHolidayCalendarYearItem, date: BankHolidayCalendarDate) {
		year.Dates.splice(year.Dates.indexOf(date), 1);
		year.SelectedDates.splice(year.Dates.indexOf(date), 1);
	}

	selectTab(year: BankHolidayCalendarYearItem) {
		setTimeout(() => {
			year.Active = true;
		}, 0);
	}

	deselectTab(year: BankHolidayCalendarYearItem) {
		setTimeout(() => {
			year.Active = false;
		}, 0);
	}

	onChange(selectedDate: Date) {}

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

			this.resetAddSpace();
		});
	}

	resetAddSpace() {
		this.newCalendarName = null;
		this.newCalendarYear = null;
		this.newCalendarYears = [];
	}
}
