import { Component, OnInit, Input } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NzNotificationService, NzModalService } from 'ng-zorro-antd';
import {
	BankHolidayCalendar,
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
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
export class BankHolidayCalendarAddComponent implements OnInit {
	@Input() exit: Function;

	yearFormat = this.bankCalendarDataService.yearFormat;
	dateFormat = this.bankCalendarDataService.dateFormat;
	bankHolidayCalendarsList: BankHolidayCalendarItem[];
	newCalendarName = '';
	nameAlreadyExisting = false;
	selectedYearDate: Date;
	newCalendarYears: BankHolidayCalendarYearItem[] = [];
	newCalendarTabIndex: number;
	activedYearTab: BankHolidayCalendarYearItem;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private menuService: ToggleMenuService,
		private modalService: NzModalService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.bankHolidayCalendarsList$.subscribe(calendars => {
			this.bankHolidayCalendarsList = calendars;
		});

		this.menuService.showMenu$.subscribe(isMenuVisible => {
			if (this.activedYearTab) {
				this.activedYearTab.Active = false;
			}
			setTimeout(() => {
				if (this.activedYearTab) {
					this.activedYearTab.Active = true;
				}
			});
		});
	}

	checkNewCalendarName() {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(
			c => c.Name.trim() === this.newCalendarName.trim()
		);
	}

	newYearTab(date: Date): void {
		const newCalendarYearDate = new Date(
			moment(date, this.dateFormat)
				.startOf('year')
				.format(this.dateFormat)
		);
		const yearStr = moment(newCalendarYearDate, this.dateFormat).format(this.yearFormat);
		if (this.newCalendarYears.some(y => y.Year === yearStr)) {
			return;
		}

		this.newCalendarYears.forEach(y => (y.Active = false));

		const newYear = {
			Year: yearStr,
			YearDate: new Date(yearStr),
			DisabledDate: d => {
				return (
					moment(d, this.dateFormat) < moment(yearStr, this.dateFormat).startOf('year') ||
					moment(d, this.dateFormat) > moment(yearStr, this.dateFormat).endOf('year')
				);
			},
			Active: true,
			Dates: [],
			ModifiedDates: [],
			SelectedDates: []
		};
		this.activedYearTab = newYear;
		this.newCalendarYears.push(newYear);
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
	}

	confirmDeleteYearTab(year: BankHolidayCalendarYearItem) {
		this.modalService.confirm({
			nzTitle: this.translate
				.instant('AreYouSureToDeleteYearFromCalendar')
				.replace('{0}', year.Year.toString())
				.replace('{1}', this.newCalendarName),
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				return this.deleteYearTab(year);
			},
			nzOnCancel: () => {}
		});
	}

	deleteYearTab(year: BankHolidayCalendarYearItem): boolean {
		this.newCalendarYears.splice(this.newCalendarYears.indexOf(year), 1);
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
		if (this.newCalendarYears[this.newCalendarTabIndex])
			this.newCalendarYears[this.newCalendarTabIndex].Active = true;

		return true;
	}

	dateClick(currentDate: any, year: BankHolidayCalendarYearItem) {
		setTimeout(() => {
			if (
				moment(currentDate.nativeDate, this.dateFormat) < moment(year.Year, this.yearFormat).startOf('year') ||
				moment(currentDate.nativeDate, this.dateFormat) > moment(year.Year, this.yearFormat).endOf('year')
			) {
				return;
			}
			this.dateChangeCallback(year.YearDate, year);
		});
	}

	dateChangeCallback(date: Date, year: BankHolidayCalendarYearItem) {
		year.Dates.forEach(d => (d.IsLastAdded = false));
		const index = year.SelectedDates.indexOf(date.getTime());

		if (index > -1) {
			year.Dates[index].IsLastAdded = true;
			return;
		}

		this.addDateForYear(date, year);
	}

	addDateForYear(date: Date, year: BankHolidayCalendarYearItem) {
		const newDate: BankHolidayCalendarDateItem = {
			Date: moment(date, this.dateFormat).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday'),
			IsLastAdded: true
		};

		if (year.ModifiedDates) {
			const modifiedDate = year.ModifiedDates.filter(d => {
				return d.Date === newDate.Date;
			})[0];
			if (modifiedDate) {
				modifiedDate.IsDeleted = false;
				modifiedDate.Description = newDate.Description;
				newDate.Id = modifiedDate.Id;
			} else {
				year.ModifiedDates.push(newDate);
			}
		}

		year.Dates.push(newDate);
		year.Dates = [
			...year.Dates.sort((c, n) => {
				return moment(c.Date, this.dateFormat) < moment(n.Date, this.dateFormat) ? -1 : 1;
			})
		];

		year.SelectedDates.push(date.getTime());
		year.SelectedDates = [...year.SelectedDates.sort()];
	}

	removeDateOfYear(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYearItem) {
		const index = year.Dates.indexOf(date);
		const deletedDate = year.Dates.splice(index, 1)[0];
		if (deletedDate.Id) {
			deletedDate.IsDeleted = true;
			const modifiedDate = year.ModifiedDates.filter(d => {
				return d.Date === deletedDate.Date;
			})[0];

			if (modifiedDate) modifiedDate.IsDeleted = true;
			else year.ModifiedDates.push(deletedDate);
		} else {
			year.ModifiedDates.splice(year.ModifiedDates.indexOf(deletedDate), 1);
		}

		year.SelectedDates.splice(index, 1);
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

	buildYearsForPost(years: BankHolidayCalendarYearItem[]): BankHolidayCalendarYear[] {
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
