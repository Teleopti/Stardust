import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs/';
import { NzNotificationService } from 'ng-zorro-antd';
import {
	BankHolidayCalendar,
	BankHolidayCalendarYear,
	BankHolidayCalendarDateItem,
	BankHolidayCalendarItem
} from '../../interface';
import { BankCalendarDataService } from '../../shared';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';

@Component({
	selector: 'bank-holiday-calendar-add',
	templateUrl: './bank-holiday-calendar-add.component.html',
	styleUrls: ['./bank-holiday-calendar-add.component.scss']
})
export class BankHolidayCalendarAddComponent implements OnInit, OnDestroy {
	@Input() exit: Function;

	yearFormat = this.bankCalendarDataService.yearFormat;
	dateFormat = this.bankCalendarDataService.dateFormat;
	bankHolidayCalendarsList: BankHolidayCalendarItem[];

	newCalendarName = '';
	nameAlreadyExisting = false;

	selectedYearDate: Date;
	newCalendarYears: BankHolidayCalendarYear[] = [];
	selectedDates: BankHolidayCalendarDateItem[] = [];
	selectedDatesTimeList: number[] = [];
	showDatePicker: boolean;
	menuSubscription: Subscription;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private menuService: ToggleMenuService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.bankHolidayCalendarsList$.subscribe(calendars => {
			this.bankHolidayCalendarsList = calendars;
		});

		this.menuSubscription = this.menuService.showMenu$.subscribe(isMenuVisible => {
			this.showDatePicker = false;
			setTimeout(() => {
				this.showDatePicker = true;
			});
		});
	}

	ngOnDestroy(): void {
		this.menuSubscription.unsubscribe();
	}

	checkNewCalendarName() {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(
			c => c.Name.trim() === this.newCalendarName.trim()
		);
	}

	dateClick(currentDate: any) {
		setTimeout(() => {
			this.dateChangeCallback(currentDate.nativeDate);
		});
	}

	dateChangeCallback(date: Date) {
		const dateString = moment(date, this.dateFormat).format(this.dateFormat);
		if (this.selectedDates.filter(item => item.Date === dateString).length > 0) return;

		this.newCalendarYears.forEach(year => {
			year.Dates.forEach(d => {
				d.IsLastAdded = false;
			});
		});

		const newDate: BankHolidayCalendarDateItem = {
			Date: dateString,
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
		};
		this.selectedDates.push(newDate);
		this.selectedDates = [...this.selectedDates];

		this.selectedDatesTimeList.push(date.getTime());

		const yearString = moment(dateString)
			.year()
			.toString();

		const curYearItem = this.newCalendarYears.filter(year => {
			return year.Year === yearString;
		});

		if (curYearItem.length === 0) {
			const year: BankHolidayCalendarYear = {
				Year: yearString,
				Dates: [newDate],
				Active: true
			};
			this.newCalendarYears.push(year);
			this.newCalendarYears.sort((cYear, nYear) => {
				return moment(cYear.Year, this.yearFormat) < moment(nYear.Year, this.yearFormat) ? -1 : 0;
			});
		} else {
			curYearItem[0].Dates.push(newDate);
			curYearItem[0].Active = true;
			curYearItem[0].Dates = [...curYearItem[0].Dates];
		}
		this.newCalendarYears.forEach(y => {
			y.Dates.sort((a, b) => {
				return new Date(a.Date).getTime() - new Date(b.Date).getTime();
			});
		});
	}

	collapseStatusChange(status: boolean, year: BankHolidayCalendarYear) {
		year.Active = status;
	}

	removeDate(date: BankHolidayCalendarDateItem) {
		const index = this.selectedDates.indexOf(date);
		this.selectedDates.splice(index, 1);
	}

	saveNewBankCalendar() {
		this.newCalendarYears.sort((c, n) => {
			return moment(c.Year, this.yearFormat) < moment(n.Year, this.yearFormat) ? -1 : 1;
		});

		const bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.buildYearsForPost(this.newCalendarYears)
		};

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			if (result.Id.length > 0) {
				const calItem = result as BankHolidayCalendarItem;
				const curYear = moment().year();

				calItem.CurrentYearIndex = 0;
				calItem.Years.forEach((y, i) => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date, this.dateFormat).format(this.dateFormat);
					});

					if (moment(y.Year.toString(), this.yearFormat).year() === curYear) {
						calItem.CurrentYearIndex = i;
					}
				});

				this.bankHolidayCalendarsList.unshift(calItem);
				this.bankHolidayCalendarsList.sort((c, n) => {
					return c.Name.localeCompare(n.Name);
				});

				this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);
				this.exit();
			} else {
				this.networkError();
			}
		}, this.networkError);
	}

	buildYearsForPost(years: BankHolidayCalendarYear[]): BankHolidayCalendarYear[] {
		const result: BankHolidayCalendarYear[] = [];
		years.forEach(y => {
			const dates = [...y.Dates];
			dates.forEach(d => {
				delete d.IsLastAdded;
			});

			result.push({
				Year: y.Year,
				Dates: dates
			});
		});
		return result.filter(y => y.Dates.length > 0);
	}

	networkError = (error?: any) => {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
