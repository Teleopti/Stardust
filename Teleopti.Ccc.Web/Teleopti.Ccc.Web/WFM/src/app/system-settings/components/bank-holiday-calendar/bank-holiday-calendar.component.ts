import { Component, OnInit } from '@angular/core';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';
import { BankHolidayCalendarListItem } from '../../interface';
import { NzModalService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

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
	isEdittingCalendar: boolean = false;

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
			nzContent: this.translate.instant('Name') + ': ' + calendar.Name,
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				this.deleteHolidayCalendar(calendar);
			}
		});
	}

	deleteHolidayCalendar(calendar: BankHolidayCalendarListItem) {
		this.bankCalendarDataService.deleteBankHolidayCalendar(calendar.Id).subscribe(result => {
			if (result) {
				this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(calendar), 1);
			}
		});
	}

	startAddNewBankCalender() {
		this.isAddingNewCalendar = true;
	}

	exitAddNewBankCalendar = () => {
		this.isAddingNewCalendar = false;
	};
}
