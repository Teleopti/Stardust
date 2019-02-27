import { Component, OnInit, Input, OnDestroy, ElementRef, AfterViewInit, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { NzNotificationService } from 'ng-zorro-antd';
import { BankHolidayCalendar, BankHolidayCalendarYear, BankHolidayCalendarDateItem } from '../../interface';
import { BankCalendarDataService } from '../../shared';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { DeepCopyService } from 'src/app/shared/services/deep-copy.service';

@Component({
	selector: 'bank-holiday-calendar-add',
	templateUrl: './bank-holiday-calendar-add.component.html',
	styleUrls: ['./bank-holiday-calendar-add.component.scss']
})
export class BankHolidayCalendarAddComponent implements OnInit, OnDestroy, AfterViewInit {
	@Input() exit: Function;
	@ViewChild('calendarNameInput') calendarNameInputElment: ElementRef;

	yearFormat = this.bankCalendarDataService.yearFormat;
	dateFormat = this.bankCalendarDataService.dateFormat;

	newCalendarName = '';
	nameAlreadyExisting = false;
	isNewCalendarNameEmpty = false;
	newCalendarId = null;

	showDatePicker: boolean;
	selectedYearDate: Date;
	newCalendarYears: BankHolidayCalendarYear[] = [];
	newCalendarYearsForDisplay: BankHolidayCalendarYear[] = [];
	selectedDatesTimeList: number[] = [];
	bankHolidayCalendarsList: BankHolidayCalendar[] = [];

	menuSubscription: Subscription;
	bankHolidayListSubscription: Subscription;
	timestampListSubscription: Subscription;
	isSavingCalendar = false;

	constructor(
		public bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private menuService: ToggleMenuService,
		private copyService: DeepCopyService
	) {}

	ngOnInit(): void {
		this.newCalendarName = this.translate.instant('BankHolidayCalendar') + moment().format(this.dateFormat);

		this.bankHolidayListSubscription = this.bankCalendarDataService.bankHolidayCalendarsList$.subscribe(
			calendars => {
				this.bankHolidayCalendarsList = calendars;
			}
		);

		this.menuSubscription = this.menuService.showMenu$.subscribe(isMenuVisible => {
			this.showDatePicker = false;
			setTimeout(() => {
				this.showDatePicker = true;
			});
		});
	}

	ngAfterViewInit(): void {
		setTimeout(() => {
			this.calendarNameInputElment.nativeElement.select();
		}, 0);
	}

	ngOnDestroy(): void {
		this.bankHolidayListSubscription.unsubscribe();
		this.menuSubscription.unsubscribe();
	}

	saveNewCalendarName() {
		if (this.checkNameAlreadyExisting() || this.checkNameIsEmpty()) {
			return;
		}

		this.saveNewBankCalendar();
	}

	checkNameAlreadyExisting(): boolean {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(c => {
			return c.Id !== this.newCalendarId && c.Name.trim() === this.newCalendarName.trim();
		});

		return this.nameAlreadyExisting;
	}

	checkNameIsEmpty(): boolean {
		this.isNewCalendarNameEmpty = !this.newCalendarName;
		return this.isNewCalendarNameEmpty;
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
		if (this.checkNameAlreadyExisting() || this.checkNameIsEmpty()) {
			return;
		}
		const dateString = moment(date, this.dateFormat).format(this.dateFormat);
		const timeStamp = new Date(dateString).getTime();
		if (this.selectedDatesTimeList.filter(time => time === timeStamp).length > 0) return;

		this.resetLastAddedDateItem();

		const newDate: BankHolidayCalendarDateItem = {
			Date: dateString,
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
		};

		this.addDateToYear(newDate);
		this.saveNewBankCalendar(newDate);
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
			if (curYearItem[0].Dates.filter(date => date.Date === newDate.Date).length > 0) return;

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

	removeDate(date: BankHolidayCalendarDateItem) {
		date.IsDeleted = true;

		this.saveNewBankCalendar(date);
	}

	sortDateOrYearAscending(currentDate: any, nextDate: any) {
		return moment(currentDate, this.dateFormat) < moment(nextDate, this.dateFormat) ? -1 : 0;
	}

	saveDescription(newDate: BankHolidayCalendarDateItem) {
		this.saveNewBankCalendar(newDate);
	}

	saveNewBankCalendar(newDate?: BankHolidayCalendarDateItem) {
		if (this.isSavingCalendar) return;

		this.isSavingCalendar = true;

		const bankHolidayCalendar: BankHolidayCalendar = {
			Id: this.newCalendarId,
			Name: this.newCalendarName,
			Years: newDate ? this.buildYearsForPost(this.newCalendarYears, newDate) : []
		};

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			if (result.Id.length > 0) {
				this.newCalendarId = result.Id;
				this.newCalendarName = result.Name;
				bankHolidayCalendar.Id = result.Id;

				result.Years.forEach((y, i) => {
					y.Year = y.Year.toString();
					y.Dates.forEach(d => {
						d.Date = moment(d.Date, this.dateFormat).format(this.dateFormat);
						d.IsLastAdded = false;

						if (newDate && newDate.Date === d.Date) {
							d.IsLastAdded = true;
						}
					});
					y.Active = true;
				});
				this.newCalendarYearsForDisplay = this.copyService.copy(result.Years);
				this.newCalendarYears = result.Years;

				this.updateBankHolidayCalendarList(result);
				if (newDate) {
					this.updateTimeStampList(newDate);
				}
				this.isSavingCalendar = false;
			} else {
				this.networkError();
			}
		}, this.networkError);
	}

	buildYearsForPost(
		years: BankHolidayCalendarYear[],
		newDate: BankHolidayCalendarDateItem
	): BankHolidayCalendarYear[] {
		return years
			.filter(y => y.Year === moment(newDate.Date, this.dateFormat).format(this.yearFormat))
			.map(year => {
				const dates = [newDate].map(date => {
					return {
						Id: date.Id,
						Date: date.Date,
						Description: date.Description,
						IsDeleted: date.IsDeleted
					};
				});

				return {
					Year: year.Year,
					Dates: dates
				};
			});
	}

	updateBankHolidayCalendarList(bankHolidayCalendar: BankHolidayCalendar) {
		const currentCalendar = this.bankHolidayCalendarsList.filter(
			calendar => calendar.Id === bankHolidayCalendar.Id
		);
		if (currentCalendar.length === 0) {
			this.bankHolidayCalendarsList.push(bankHolidayCalendar);
			this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);
		} else {
			currentCalendar[0].Name = bankHolidayCalendar.Name;
			currentCalendar[0].Years = bankHolidayCalendar.Years;
			this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);
		}
	}

	updateTimeStampList(newDate: BankHolidayCalendarDateItem) {
		const timeStamp = new Date(newDate.Date).getTime();
		if (newDate.IsDeleted) {
			const timeStampIdx = this.selectedDatesTimeList.indexOf(timeStamp);
			if (timeStampIdx > -1) this.selectedDatesTimeList.splice(timeStampIdx, 1);
		} else {
			this.selectedDatesTimeList.push(timeStamp);
		}
	}

	networkError = (error?: any) => {
		this.isSavingCalendar = false;
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
