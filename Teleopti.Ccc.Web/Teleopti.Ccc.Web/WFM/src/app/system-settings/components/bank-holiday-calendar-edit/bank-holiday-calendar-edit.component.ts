import { Component, OnInit, Input } from '@angular/core';
import { NzNotificationService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import {
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarDateItem,
	BankHolidayCalendar
} from '../../interface';
import { BankCalendarDataService } from '../../shared';
import { ToggleMenuService } from 'src/app/menu/shared/toggle-menu.service';

@Component({
	selector: 'bank-holiday-calendar-edit',
	templateUrl: './bank-holiday-calendar-edit.component.html',
	styleUrls: ['./bank-holiday-calendar-edit.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarEditComponent implements OnInit {
	@Input() edittingCalendar: BankHolidayCalendar;
	@Input() bankHolidayCalendarsList: BankHolidayCalendar[];
	@Input() exitEdittingBankCalendar: Function;

	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	nameAlreadyExisting: boolean = false;
	selectedYearDate: Date;
	edittingCalendarName: string;
	edittingCalendarYears: BankHolidayCalendarYearItem[] = [];
	editingCalendarTabIndex: number;
	deletedYears: BankHolidayCalendarYearItem[] = [];
	isDeleteYearModalVisible: boolean = false;
	isDeleteCalendarModalVisible: boolean = false;
	activedYearTab: BankHolidayCalendarYearItem;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private menuService: ToggleMenuService
	) {}

	ngOnInit(): void {
		this.edittingCalendarName = this.edittingCalendar.Name;
		if (this.edittingCalendar.Years.length == 0) return;

		this.edittingCalendar.Years.forEach(y => {
			let year: BankHolidayCalendarYearItem = {
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
			this.edittingCalendarYears.push(year);
		});

		this.edittingCalendarYears[0].Active = true;
		this.activedYearTab = this.edittingCalendarYears[0];

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
			this.edittingCalendarName != this.edittingCalendar.Name &&
			this.bankHolidayCalendarsList.some(c => c.Name === this.edittingCalendarName);
	}

	newYearTab(date: Date): void {
		let newYearDate = new Date(
			moment(date)
				.startOf('year')
				.format(this.dateFormat)
		);
		let yearStr = moment(newYearDate).format(this.yearFormat);
		if (this.edittingCalendarYears.some(y => y.Year == yearStr)) {
			return;
		}

		this.edittingCalendarYears.forEach(y => (y.Active = false));

		let newYearItem = {
			Year: yearStr,
			YearDate: this.selectedYearDate,
			DisabledDate: date => {
				return moment(date) < moment(yearStr).startOf('year') || moment(date) > moment(yearStr).endOf('year');
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

	addNewDateForYear(date: Date, year: BankHolidayCalendarYearItem) {
		year.Dates.forEach(d => (d.IsLastAdded = false));
		let index = year.SelectedDates.indexOf(date.getTime());

		if (index > -1) {
			year.Dates[index].IsLastAdded = true;
			return;
		}

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
		year.ModifiedDates.push(newDate);
	}

	removeDateOfYear(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYearItem) {
		let index = year.Dates.indexOf(date);

		year.ModifiedDates = year.ModifiedDates.concat(year.Dates.splice(index, 1));
		year.ModifiedDates.forEach(d => {
			if (date.Id && date.Id === d.Id) d.IsDeleted = true;
		});
		year.SelectedDates.splice(index, 1);
	}

	updateDateDescription(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYearItem) {
		if (
			year.ModifiedDates.some(d => {
				return d.Date === date.Date;
			})
		) {
			year.ModifiedDates.forEach(d => {
				if (date.Date === d.Date) {
					d.Id = date.Id;
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
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Id: this.edittingCalendar.Id,
			Name: this.edittingCalendarName,
			Years: this.buildYearsForPost(this.edittingCalendarYears.concat(this.deletedYears))
		};

		this.bankCalendarDataService.saveExistingHolidayCalendar(bankHolidayCalendar).subscribe(
			result => {
				result.Years.forEach(y => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date).format(this.dateFormat);
					});
				});

				this.bankHolidayCalendarsList[this.bankHolidayCalendarsList.indexOf(this.edittingCalendar)] = result;
				this.resetEditSpace();
				this.exitEdittingBankCalendar();
			},
			error => {
				this.noticeService.error(
					this.translate.instant('Error'),
					this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
				);
			}
		);
	}

	buildYearsForPost(years: BankHolidayCalendarYearItem[]): BankHolidayCalendarYear[] {
		let result: BankHolidayCalendarYear[] = [];
		years.forEach(y => {
			let dates = [...y.ModifiedDates];
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

	resetEditSpace() {
		this.edittingCalendar = null;
		this.editingCalendarTabIndex = 0;
		this.edittingCalendarYears = [];
	}

	confirmDeleteHolidayCanlendar() {
		this.isDeleteCalendarModalVisible = true;
	}

	deleteHolidayCalendar(calendar: BankHolidayCalendar) {
		this.bankCalendarDataService.deleteBankHolidayCalendar(calendar.Id).subscribe(
			result => {
				if (result === true) {
					this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
					this.noticeService.success(
						this.translate.instant('Success'),
						this.translate
							.instant('BankHolidayCalendarHasBeenSuccessfullyDeleted')
							.replace('{0}', this.edittingCalendar.Name)
					);
					this.exitEdittingBankCalendar();
				}
			},
			error => {
				this.noticeService.error(
					this.translate.instant('Error'),
					this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
				);
			}
		);
	}

	closeDeleteCalendarModal() {
		this.isDeleteCalendarModalVisible = false;
	}
}
