import { Component, OnInit, Input, OnDestroy, ElementRef, AfterViewInit, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { NzNotificationService } from 'ng-zorro-antd';
import {
	BankHolidayCalendarItem,
	BankHolidayCalendarYear,
	BankHolidayCalendarDateItem,
	BankHolidayCalendar
} from '../../interface';
import { BankCalendarDataService } from '../../shared';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';
import { DeepCopyService } from 'src/app/shared/services/deep-copy.service';

@Component({
	selector: 'bank-holiday-calendar-edit',
	templateUrl: './bank-holiday-calendar-edit.component.html',
	styleUrls: ['./bank-holiday-calendar-edit.component.scss']
})
export class BankHolidayCalendarEditComponent implements OnInit, OnDestroy, AfterViewInit {
	@Input() exit: Function;
	@Input() edittingCalendar: BankHolidayCalendarItem;
	@ViewChild('calendarNameInput') calendarNameInputElment: ElementRef;

	yearFormat = this.bankCalendarDataService.yearFormat;
	dateFormat = this.bankCalendarDataService.dateFormat;

	newCalendar: BankHolidayCalendarItem = {
		Id: null,
		Name: '',
		Years: [],
		ActiveYearIndex: 0
	};
	calendarName = '';
	nameAlreadyExisting = false;
	isNewCalendarNameEmpty = false;
	isShowCalendarNameHint = true;
	showCalendarAutoSaveMsg = false;
	autoSaveTimer = null;
	autoSaveTimeoutMilliseconds = 2000;

	showDatePicker = true;
	selectedYearDate: Date;
	bankHolidayCalendarsList: BankHolidayCalendarItem[] = [];
	newCalendarYearsForDisplay: BankHolidayCalendarYear[] = [];
	activedYears: string[] = [];
	selectedDatesTimeList: number[] = [];

	menuSubscription: Subscription;
	bankHolidayListSubscription: Subscription;
	timestampListSubscription: Subscription;
	isSavingCalendar = false;
	originalActiveYearIndex = 0;

	constructor(
		public bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private menuService: ToggleMenuService,
		private copyService: DeepCopyService
	) {}

	ngOnInit(): void {
		if (this.edittingCalendar) {
			this.newCalendar = this.edittingCalendar;
			this.isShowCalendarNameHint = !this.newCalendar.Name;
			this.calendarName = this.edittingCalendar.Name;
			this.originalActiveYearIndex = this.edittingCalendar.ActiveYearIndex;
			this.newCalendar.Years.forEach((y, i) => {
				y.Active = i === this.newCalendar.ActiveYearIndex;

				if (y.Active) {
					this.activedYears.push(y.Year.toString());
					this.selectedYearDate = new Date(y.Year.toString());
				}
				y.Dates.forEach(date => {
					const timeStamp = new Date(date.Date).getTime();
					if (!this.selectedDatesTimeList.includes(timeStamp)) this.selectedDatesTimeList.push(timeStamp);
				});
			});
			this.newCalendarYearsForDisplay = this.copyService.copy(this.edittingCalendar.Years);
		}

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
			this.calendarNameInputElment.nativeElement.focus();
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
			return c.Id !== this.newCalendar.Id && c.Name.trim() === this.calendarName.trim();
		});
		this.isShowCalendarNameHint = !this.calendarName;
		return this.nameAlreadyExisting;
	}

	checkNameIsEmpty(): boolean {
		this.isNewCalendarNameEmpty = !this.calendarName;
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
		this.newCalendar.Years.forEach(year => {
			year.Dates.forEach(d => {
				d.IsLastAdded = false;
			});
		});
	}

	addDateToYear(newDate: BankHolidayCalendarDateItem) {
		const yearString = moment(newDate.Date)
			.year()
			.toString();

		const curYearItem = this.newCalendar.Years.filter(year => {
			return year.Year === yearString;
		});

		if (curYearItem.length === 0) {
			const year: BankHolidayCalendarYear = {
				Year: yearString,
				Dates: [newDate],
				Active: true
			};
			if (this.activedYears.indexOf(year.Year) === -1) this.activedYears.push(year.Year);
			this.newCalendar.Years.push(year);
			this.newCalendar.Years.sort((c, n) => {
				return this.sortDateOrYearAscending(c.Year, n.Year);
			});
		} else {
			const year = curYearItem[0];
			if (year.Dates.filter(date => date.Date === newDate.Date).length > 0) return;

			year.Dates.push(newDate);
			year.Dates.sort((c, n) => {
				return this.sortDateOrYearAscending(c.Date, n.Date);
			});
			year.Active = true;
			year.Dates = [...year.Dates];

			if (this.activedYears.indexOf(year.Year) === -1) this.activedYears.push(year.Year);
		}
	}

	collapseStatusChange(status: boolean, year: BankHolidayCalendarYear) {
		year.Active = status;

		const index = this.activedYears.indexOf(year.Year);
		if (index > -1) {
			this.activedYears.splice(index, 1);
		} else if (index === -1 && status) {
			this.activedYears.push(year.Year);
			this.activedYears.sort((c, n) => {
				return this.sortDateOrYearAscending(c, n);
			});
		}
	}

	removeDate(date: BankHolidayCalendarDateItem) {
		if (!this.checkNameIsEmpty()) {
			date.IsDeleted = true;
			this.saveNewBankCalendar(date);
		}
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
			Id: this.newCalendar.Id,
			Name: this.calendarName,
			Years: newDate ? this.buildYearsForPost(this.newCalendar.Years, newDate) : []
		};

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			if (result.Id.length > 0) {
				const resultCalendar = result as BankHolidayCalendarItem;
				this.formattingDataOfReturnCalendar(resultCalendar, newDate);

				this.newCalendar.Id = resultCalendar.Id;
				this.newCalendar.Name = resultCalendar.Name;
				this.newCalendar.Years = resultCalendar.Years;
				this.newCalendarYearsForDisplay = this.copyService.copy(resultCalendar.Years);

				this.updateBankHolidayCalendarList(resultCalendar);
				this.updateTimeStampList(newDate);
				this.showAutoSaveMessage();

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

	formattingDataOfReturnCalendar(resultCalendar: BankHolidayCalendarItem, newDate?: BankHolidayCalendarDateItem) {
		resultCalendar.ActiveYearIndex = this.originalActiveYearIndex;
		resultCalendar.Years.forEach((year, yearIndex) => {
			year.Year = year.Year.toString();
			year.Dates.forEach(d => {
				d.Date = moment(d.Date, this.dateFormat).format(this.dateFormat);
				d.IsLastAdded = newDate && newDate.Date === d.Date;
			});

			if (newDate && moment(newDate.Date, this.dateFormat).format(this.yearFormat) === year.Year) {
				resultCalendar.ActiveYearIndex = yearIndex;
			}
			year.Active = this.activedYears.indexOf(year.Year) > -1;
		});
	}

	updateBankHolidayCalendarList(bankHolidayCalendar: BankHolidayCalendarItem) {
		const filteredCalendars = this.bankHolidayCalendarsList.filter(
			calendar => calendar.Id === bankHolidayCalendar.Id
		);
		if (filteredCalendars.length === 0) {
			this.bankHolidayCalendarsList.push(bankHolidayCalendar);
			this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);
		} else {
			const existingCalendar = filteredCalendars[0];
			existingCalendar.Name = bankHolidayCalendar.Name;
			existingCalendar.Years = bankHolidayCalendar.Years;
			existingCalendar.ActiveYearIndex = bankHolidayCalendar.ActiveYearIndex;

			this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);
		}
	}

	showAutoSaveMessage() {
		this.showCalendarAutoSaveMsg = true;
		if (this.autoSaveTimer) clearTimeout(this.autoSaveTimer);
		this.autoSaveTimer = setTimeout(_ => {
			this.showCalendarAutoSaveMsg = false;
			this.autoSaveTimer = null;
		}, this.autoSaveTimeoutMilliseconds);
	}

	updateTimeStampList(newDate?: BankHolidayCalendarDateItem) {
		if (!newDate) return;

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
		this.calendarName = this.newCalendar.Name;
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
