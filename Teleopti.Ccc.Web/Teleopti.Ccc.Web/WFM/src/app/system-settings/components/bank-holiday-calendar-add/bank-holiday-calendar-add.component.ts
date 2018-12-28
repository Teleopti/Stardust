import { Component, OnInit, Input } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import {
	BankHolidayCalendar,
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarDateItem
} from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-add',
	templateUrl: './bank-holiday-calendar-add.component.html',
	styleUrls: ['./bank-holiday-calendar-add.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarAddComponent implements OnInit {
	@Input() bankHolidayCalendarsList: BankHolidayCalendar[];
	@Input() exitAddNewBankCalendar: Function;

	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	newCalendarName: string = '';
	nameAlreadyExisting: boolean = false;
	selectedYear: Date;
	newCalendarYears: BankHolidayCalendarYearItem[] = [];
	newCalendarTabIndex: number;

	constructor(private bankCalendarDataService: BankCalendarDataService, private translate: TranslateService) {}

	ngOnInit(): void {}

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
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
		this.newCalendarYears[this.newCalendarTabIndex].Active = true;
	}

	addNewDateForYear(date: Date, year: BankHolidayCalendarYearItem) {
		if (year.SelectedDates.indexOf(date.getTime()) > -1) return;

		year.Dates.forEach(d => (d.IsLastAdded = false));

		let newDate: BankHolidayCalendarDateItem = {
			Date: moment(date).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
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

	removeDateOfYear(year: BankHolidayCalendarYearItem, date: BankHolidayCalendarDateItem) {
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

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			if (result.Id.length > 0) {
				this.bankHolidayCalendarsList.unshift(result);
				this.bankHolidayCalendarsList.sort((c, n) => {
					return c.Name.localeCompare(n.Name);
				});
				this.exitAddNewBankCalendar();
			}
		});
	}
}
