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
	newCalendarYearsForDisplay: BankHolidayCalendarYear[] = [];
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

	highlightDate(currentDate: any): boolean {
		const jsDate = currentDate.nativeDate;
		const month = jsDate.getMonth() + 1;
		const date = jsDate.getDate() > 9 ? jsDate.getDate() : '0' + jsDate.getDate();
		const dateString = jsDate.getFullYear() + '-' + (month > 9 ? month : '0' + month) + '-' + date;

		return this.selectedDatesTimeList.indexOf(new Date(dateString).getTime()) > -1;
	}

	dateClick(currentDate: any) {
		setTimeout(() => {
			this.dateChangeCallback(currentDate.nativeDate);
		});
	}

	dateChangeCallback(date: Date) {
		const dateString = moment(date, this.dateFormat).format(this.dateFormat);
		const timeStamp = new Date(dateString).getTime();

		if (this.selectedDatesTimeList.filter(time => time === timeStamp).length > 0) return;

		this.resetLastAddedDateItem();

		const newDate: BankHolidayCalendarDateItem = {
			Date: dateString,
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
		};

		this.selectedDatesTimeList.push(timeStamp);

		this.addDateToYear(newDate);
		this.saveNewBankCalendar();
	}

	resetLastAddedDateItem() {
		this.newCalendarYears.forEach(year => {
			year.Dates.forEach(d => {
				d.IsLastAdded = false;
			});
		});
	}

	addDateToYear(newDate: BankHolidayCalendarDateItem) {
		const yearString = moment(newDate.Date)
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
			this.newCalendarYears.sort((c, n) => {
				return this.sortDateOrYearAscending(c.Year, n.Year);
			});
		} else {
			curYearItem[0].Dates.push(newDate);
			curYearItem[0].Dates.sort((c, n) => {
				return this.sortDateOrYearAscending(c.Date, n.Date);
			});
			curYearItem[0].Active = true;
			curYearItem[0].Dates = [...curYearItem[0].Dates];
		}
	}

	collapseStatusChange(status: boolean, year: BankHolidayCalendarYear) {
		year.Active = status;
	}

	removeDate(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYear) {
		date.IsDeleted = true;

		const timeStampIdx = this.selectedDatesTimeList.indexOf(new Date(date.Date).getTime());
		this.selectedDatesTimeList.splice(timeStampIdx, 1);

		this.saveNewBankCalendar();
	}

	sortDateOrYearAscending(currentDate: any, nextDate: any) {
		return moment(currentDate, this.dateFormat) < moment(nextDate, this.dateFormat) ? -1 : 0;
	}

	saveNewBankCalendar() {
		const bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.buildYearsForPost(this.newCalendarYears)
		};

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			if (result.Id.length > 0) {
				const calItem = result as BankHolidayCalendarItem;
				calItem.Years.forEach((y, i) => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date, this.dateFormat).format(this.dateFormat);
					});
					y.Active = true;
				});
				this.newCalendarYearsForDisplay = calItem.Years;
				this.newCalendarYears = calItem.Years.concat();
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
