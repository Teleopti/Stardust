import { Component, OnInit, Input } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NzModalService, NzNotificationService } from 'ng-zorro-antd';
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

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private modalService: NzModalService,
		private noticeService: NzNotificationService
	) {}

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
			YearDate: new Date(yearStr),
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

	confirmDeleteYearTab(year: BankHolidayCalendarYearItem) {
		setTimeout(() => {
			year.Active = false;

			this.modalService.confirm({
				nzTitle: this.translate
					.instant('AreYouSureToDeleteYearFromCalendar')
					.replace('{0}', year.Year.toString())
					.replace('{1}', this.newCalendarName),
				nzOkType: 'danger',
				nzOkText: this.translate.instant('Delete'),
				nzCancelText: this.translate.instant('Cancel'),
				nzOnOk: () => {
					this.deleteYearTab(year);
				},
				nzOnCancel: () => {
					setTimeout(() => {
						year.Active = true;
					}, 0);
				}
			});
		}, 0);
	}

	deleteYearTab(year: BankHolidayCalendarYearItem) {
		this.newCalendarYears.splice(this.newCalendarYears.indexOf(year), 1);
		this.newCalendarTabIndex = this.newCalendarYears.length - 1;
		this.newCalendarYears[this.newCalendarTabIndex].Active = true;
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
	}

	removeDateOfYear(date: BankHolidayCalendarDateItem, year: BankHolidayCalendarYearItem) {
		let index = year.Dates.indexOf(date);
		year.Dates.splice(index, 1);
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
			return moment(c.Year) < moment(n.Year) ? -1 : 1;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.buildYearsForPost(this.newCalendarYears)
		};

		this.bankCalendarDataService.saveNewBankHolidayCalendar(bankHolidayCalendar).subscribe(
			result => {
				if (result.Id.length > 0) {
					result.Years.forEach(y => {
						y.Dates.forEach(d => {
							d.Date = moment(d.Date).format(this.dateFormat);
						});
					});

					this.bankHolidayCalendarsList.unshift(result);
					this.bankHolidayCalendarsList.sort((c, n) => {
						return c.Name.localeCompare(n.Name);
					});
					this.exitAddNewBankCalendar();
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

	buildYearsForPost(years: BankHolidayCalendarYearItem[]): BankHolidayCalendarYear[] {
		let result: BankHolidayCalendarYear[] = [];
		years.forEach(y => {
			let dates = [...y.Dates];
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
}
