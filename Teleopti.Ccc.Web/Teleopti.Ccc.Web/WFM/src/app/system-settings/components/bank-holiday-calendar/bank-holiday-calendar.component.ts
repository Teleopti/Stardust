import { Component, OnInit, Pipe, PipeTransform } from '@angular/core';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';
import { BankHolidayCalendar, BankHolidayCalendarYear, BankHolidayCalendarDate } from '.';
import { NzModalService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

export interface BankHolidayCalendarListItem extends BankHolidayCalendar {
	SelectedTabIndex: number;
}

@Component({
	selector: 'bank-holiday-calendar',
	templateUrl: './bank-holiday-calendar.component.html',
	styleUrls: ['./bank-holiday-calendar.component.scss'],
	providers: [BankCalendarDataService]
})
export class BankHolidayCalendarComponent implements OnInit {
	yearFormat: string = 'YYYY';
	dateFormat: string = 'YYYY-MM-DD';

	bankHolidayCalendarsList: BankHolidayCalendarListItem[] = [];

	isAddingNewCalendar: boolean = false;

	newCalendarName: string;
	newCalendarYear: Date;
	newCalendarYears: BankHolidayCalendarYear[] = [];
	newCalendarCurrentTabIndex = 0;
	nameAlreadyExisting: boolean = false;

	isEditingCalendar: boolean = false;
	editingCalendar: BankHolidayCalendarListItem = null;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.getBankHolidayCalendars().subscribe(calendars => {
			this.bankHolidayCalendarsList = calendars.map(c => {
				let item: BankHolidayCalendarListItem = {
					Id: c.Id,
					Name: c.Name,
					Years: c.Years,
					SelectedTabIndex: 0
				};
				return item;
			});
		});
	}

	confirmDeleteHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.modalService.confirm({
			nzTitle: this.translate.instant('AreYouSureToDeleteThisBankHolidayCalendar'),
			nzContent: this.translate.instant('Name') + calendar.Name,
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				this.deleteHolidayCanlendar(calendar);
			}
		});
	}

	deleteHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
	}

	editHolidayCanlendar(calendar: BankHolidayCalendarListItem) {
		this.editingCalendar = calendar;
		this.isEditingCalendar = true;
	}

	startAddNewBankCalender() {
		this.isAddingNewCalendar = true;
	}

	checkNewCalendarName() {
		this.nameAlreadyExisting = this.bankHolidayCalendarsList.some(c => c.Name === this.newCalendarName);
	}

	newYearTab(): void {
		if (this.newCalendarYears.some(y => y.Year == moment(this.newCalendarYear).format(this.yearFormat))) {
			return;
		}

		let newYear: BankHolidayCalendarYear = {
			Year: moment(this.newCalendarYear).format(this.yearFormat),
			Dates: [
				{
					Date: moment(this.newCalendarYear).format(this.dateFormat),
					Description: this.translate.instant('BankHoliday')
				}
			]
		};

		this.newCalendarCurrentTabIndex = this.newCalendarYears.push(newYear) - 1;
	}

	deleteYearTab(year: BankHolidayCalendarYear): void {
		this.newCalendarYears.splice(this.newCalendarYears.indexOf(year), 1);
		this.newCalendarCurrentTabIndex = this.newCalendarYears.length - 1;
	}

	addNewDateForYear(year: BankHolidayCalendarYear) {
		let newDate: BankHolidayCalendarDate = {
			Date: moment(year.Year).format(this.dateFormat),
			Description: this.translate.instant('BankHoliday')
		};
		year.Dates = year.Dates.concat([newDate]);
	}

	removeDateOfYear(year: BankHolidayCalendarYear, date: BankHolidayCalendarDate) {
		year.Dates.splice(year.Dates.indexOf(date), 1);
	}

	onChange(selectedDate: Date) {}

	cancelAddNewBankCalendar() {
		this.isAddingNewCalendar = false;
	}

	saveNewBankCalendar() {
		let bankHolidayCalendar: BankHolidayCalendar = {
			Name: this.newCalendarName,
			Years: this.newCalendarYears.sort((c, n) => {
				return moment(c.Year) < moment(n.Year) ? -1 : 1;
			})
		};

		this.bankCalendarDataService.saveNewBankCalendar(bankHolidayCalendar).subscribe(result => {
			let item: BankHolidayCalendarListItem = {
				Id: result.Id,
				Name: result.Name,
				Years: result.Years,
				SelectedTabIndex: 0
			};

			this.bankHolidayCalendarsList.push(item);
			this.isAddingNewCalendar = false;

			this.resetEditSpace();
		});
	}

	resetEditSpace() {
		this.newCalendarName = null;
		this.newCalendarYear = null;
		this.newCalendarYears = [];
		this.newCalendarCurrentTabIndex = 0;
	}
}
