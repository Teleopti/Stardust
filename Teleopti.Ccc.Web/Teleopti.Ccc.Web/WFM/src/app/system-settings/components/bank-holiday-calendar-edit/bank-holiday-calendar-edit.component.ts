import { Component, OnInit, Input } from '@angular/core';
import { NzNotificationService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import {
	BankHolidayCalendar,
	BankHolidayCalendarItem,
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarDateItem
} from '../../interface';
import { BankCalendarDataService } from '../../shared';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';

@Component({
	selector: 'bank-holiday-calendar-edit',
	templateUrl: './bank-holiday-calendar-edit.component.html',
	styleUrls: ['./bank-holiday-calendar-edit.component.scss']
})
export class BankHolidayCalendarEditComponent implements OnInit {
	@Input() edittingCalendar: BankHolidayCalendarItem;
	@Input() exit: Function;

	bankHolidayCalendarsList: BankHolidayCalendarItem[];
	nameAlreadyExisting = false;
	selectedYearDate: Date;
	edittingCalendarName;
	edittingCalendarYears: BankHolidayCalendarYearItem[] = [];
	editingCalendarTabIndex: number;
	deletedYears: BankHolidayCalendarYearItem[] = [];
	isDeleteYearModalVisible = false;
	activedYearTab: BankHolidayCalendarYearItem;

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

		this.edittingCalendarName = this.edittingCalendar.Name;
		if (this.edittingCalendar.Years.length === 0) return;

		this.edittingCalendar.Years.forEach(y => {
			const year: BankHolidayCalendarYearItem = {
				Year: y.Year,
				YearDate: new Date(y.Year.toString()),
				Dates: y.Dates,
				ModifiedDates: [],
				DisabledDate: date => {
					return (
						moment(date) < moment(year.YearDate).startOf('year') ||
						moment(date) > moment(year.YearDate).endOf('year')
					);
				},
				SelectedDates: y.Dates.map(d => {
					return new Date(d.Date).getTime();
				}),
				Active: false
			};

			if (y.Dates && y.Dates.length > 0) {
				year.YearDate = new Date(y.Dates[0].Date);
			}
			this.edittingCalendarYears.push(year);
		});

		this.editingCalendarTabIndex = this.edittingCalendar.CurrentYearIndex;
		this.edittingCalendarYears[this.editingCalendarTabIndex].Active = true;
		this.activedYearTab = this.edittingCalendarYears[this.editingCalendarTabIndex];

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
		this.nameAlreadyExisting =
			this.edittingCalendarName !== this.edittingCalendar.Name &&
			this.bankHolidayCalendarsList.some(c => c.Name.trim() === this.edittingCalendarName.trim());
	}

	newYearTab(date: Date): void {
		const newYearDate = new Date(
			moment(date)
				.startOf('year')
				.format(this.bankCalendarDataService.dateFormat)
		);
		const yearStr = moment(newYearDate).format(this.bankCalendarDataService.yearFormat);
		if (this.edittingCalendarYears.some(y => y.Year === yearStr)) {
			return;
		}

		this.edittingCalendarYears.forEach(y => (y.Active = false));

		const newYearItem = {
			Year: yearStr,
			YearDate: this.selectedYearDate,
			DisabledDate: d => {
				return (
					moment(d) < moment(yearStr, 'YYYY').startOf('year') ||
					moment(d) > moment(yearStr, 'YYYY').endOf('year')
				);
			},
			Active: true,
			Dates: [],
			ModifiedDates: [],
			SelectedDates: []
		};
		this.activedYearTab = newYearItem;
		this.edittingCalendarYears.push(newYearItem);
		this.editingCalendarTabIndex = this.edittingCalendarYears.length - 1;
	}

	confirmDeleteYearTab(year: BankHolidayCalendarYearItem) {
		this.activedYearTab = year;
		this.isDeleteYearModalVisible = true;
	}

	deleteYearTab(year: BankHolidayCalendarYearItem) {
		this.isDeleteYearModalVisible = false;

		year.Dates.forEach(d => (d.IsDeleted = true));
		year.ModifiedDates = year.Dates;
		this.deletedYears.push(year);

		this.edittingCalendarYears.splice(this.edittingCalendarYears.indexOf(year), 1);
		this.editingCalendarTabIndex = this.edittingCalendarYears.length - 1;
		if (this.editingCalendarTabIndex >= 0) this.edittingCalendarYears[this.editingCalendarTabIndex].Active = true;
	}

	closeDeleteYearTabModal() {
		this.isDeleteYearModalVisible = false;
	}

	dateClick(year: BankHolidayCalendarYearItem) {
		setTimeout(() => {
			if (!year.Dates || year.Dates.length > 0) return;

			this.addDateForYear(year.YearDate, year);
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
			Date: moment(date).format(this.bankCalendarDataService.dateFormat),
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
		if (year.Dates[0]) {
			const lastAddedItem = year.Dates.filter(d => {
				return d.IsLastAdded;
			})[0];
			if (lastAddedItem) year.YearDate = new Date(lastAddedItem.Date);
			else year.YearDate = new Date(year.Dates[0].Date);
		}
	}

	updateDateDescription(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYearItem) {
		if (
			year.ModifiedDates.some(d => {
				return d.Date === date.Date;
			})
		) {
			year.ModifiedDates.forEach(d => {
				if (date.Date === d.Date) {
					d.Id = date.Id || d.Id;
					d.Description = date.Description;
				}
			});
		} else {
			year.ModifiedDates.push(date);
		}
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

	saveEditingBankCalendar() {
		this.edittingCalendarYears.sort((c, n) => {
			return moment(c.Year, 'YYYY') < moment(n.Year, 'YYYY') ? -1 : 1;
		});

		const bankHolidayCalendar: BankHolidayCalendar = {
			Id: this.edittingCalendar.Id,
			Name: this.edittingCalendarName,
			Years: this.buildYearsForPost(this.edittingCalendarYears.concat(this.deletedYears))
		};

		this.bankCalendarDataService.saveExistingHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			const calItem = result as BankHolidayCalendarItem;
			const curYear = moment().year();

			calItem.CurrentYearIndex = 0;
			calItem.Years.forEach((y, i) => {
				y.Dates.forEach(d => {
					d.Date = moment(d.Date).format(this.bankCalendarDataService.dateFormat);
				});

				if (moment(y.Year.toString(), 'YYYY').year() === curYear) {
					calItem.CurrentYearIndex = i;
				}
			});

			this.bankHolidayCalendarsList[this.bankHolidayCalendarsList.indexOf(this.edittingCalendar)] = calItem;
			this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);

			this.exit();
		}, this.networkError);
	}

	buildYearsForPost(years: BankHolidayCalendarYearItem[]): BankHolidayCalendarYear[] {
		const result: BankHolidayCalendarYear[] = [];
		years.forEach(y => {
			const dates = [...y.ModifiedDates];
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
