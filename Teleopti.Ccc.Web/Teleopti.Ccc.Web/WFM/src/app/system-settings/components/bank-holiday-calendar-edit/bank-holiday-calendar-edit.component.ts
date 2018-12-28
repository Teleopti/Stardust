import { Component, OnInit, Input } from '@angular/core';
import { NzModalService, NzNotificationService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import {
	BankHolidayCalendarYear,
	BankHolidayCalendarYearItem,
	BankHolidayCalendarDateItem,
	BankHolidayCalendar
} from '../../interface';
import { BankCalendarDataService } from '../../shared';

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
	selectedYear: Date;
	edittingCalendarName: string;
	edittingCalendarYears: BankHolidayCalendarYearItem[] = [];
	editingCalendarTabIndex: number;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService,
		private noticeService: NzNotificationService
	) {}

	ngOnInit(): void {
		this.edittingCalendarName = this.edittingCalendar.Name;

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
			YearDate: this.selectedYear,
			DisabledDate: date => {
				return moment(date) < moment(yearStr).startOf('year') || moment(date) > moment(yearStr).endOf('year');
			},
			Active: true,
			Dates: [],
			ModifiedDates: [],
			SelectedDates: []
		};

		this.edittingCalendarYears.push(newYearItem);
		this.editingCalendarTabIndex = this.edittingCalendarYears.length - 1;
	}

	deleteYearTab(year: BankHolidayCalendarYearItem): void {
		this.edittingCalendarYears.splice(this.edittingCalendarYears.indexOf(year), 1);
		this.editingCalendarTabIndex = this.edittingCalendarYears.length - 1;
		this.edittingCalendarYears[this.editingCalendarTabIndex].Active = true;
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

		this.edittingCalendarYears.forEach(y => {
			delete y.YearDate;
			delete y.DisabledDate;
			delete y.SelectedDates;
			delete y.Active;

			y.ModifiedDates.forEach(d => {
				delete d.IsLastAdded;
			});

			y.Dates = [...y.ModifiedDates];
			delete y.ModifiedDates;
		});

		let bankHolidayCalendar: BankHolidayCalendar = {
			Id: this.edittingCalendar.Id,
			Name: this.edittingCalendar.Name,
			Years: this.edittingCalendarYears as BankHolidayCalendarYear[]
		};

		this.bankCalendarDataService.saveExistingHolidayCalendar(bankHolidayCalendar).subscribe(result => {
			this.bankHolidayCalendarsList[this.bankHolidayCalendarsList.indexOf(this.edittingCalendar)] = result;
			this.resetEditSpace();
			this.exitEdittingBankCalendar();
		});
	}

	resetEditSpace() {
		this.edittingCalendar = null;
		this.editingCalendarTabIndex = 0;
		this.edittingCalendarYears = [];
	}

	confirmDeleteHolidayCanlendar(calendar: BankHolidayCalendar) {
		this.modalService.confirm({
			nzTitle: this.translate.instant('AreYouSureToDeleteThisBankHolidayCalendar'),
			nzContent: this.translate.instant('Name') + ': ' + calendar.Name,
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				this.deleteHolidayCanlendar(calendar);
			}
		});
	}

	deleteHolidayCanlendar(calendar: BankHolidayCalendar) {
		this.bankCalendarDataService.deleteBankHolidayCalendar(calendar.Id).subscribe(result => {
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
		});
	}
}
