import { Component, OnInit } from '@angular/core';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';
import { BankHolidayCalendar } from '../../interface';
import { NzModalService, NzNotificationService } from 'ng-zorro-antd';
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

	bankHolidayCalendarsList: BankHolidayCalendar[] = [];
	isAddingNewCalendar: boolean = false;
	isEdittingCalendar: boolean = false;
	edittingCalendar: BankHolidayCalendar;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private modalService: NzModalService,
		private translate: TranslateService,
		private noticeService: NzNotificationService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.getBankHolidayCalendars().subscribe(calendars => {
			calendars.forEach(c => {
				c.Years.forEach(y => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date).format(this.dateFormat);
					});
				});
			});

			this.bankHolidayCalendarsList = calendars.sort((c, n) => {
				return c.Name.localeCompare(n.Name);
			});
		});
	}

	confirmDeleteHolidayCanlendar(calendar: BankHolidayCalendar) {
		this.modalService.confirm({
			nzTitle: this.translate.instant('AreYouSureToDeleteThisBankHolidayCalendar'),
			nzContent: calendar.Name,
			nzOkType: 'danger',
			nzOkText: this.translate.instant('Delete'),
			nzCancelText: this.translate.instant('Cancel'),
			nzOnOk: () => {
				this.deleteHolidayCalendar(calendar);
			}
		});
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
							.replace('{0}', calendar.Name)
					);
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

	startAddNewBankCalender() {
		this.isAddingNewCalendar = true;
	}

	exitAddNewBankCalendar = () => {
		this.isAddingNewCalendar = false;
	};

	startEditBankCalendar(calendar: BankHolidayCalendar) {
		this.edittingCalendar = calendar;
		this.isEdittingCalendar = true;
	}

	exitEdittingBankCalendar = () => {
		this.isEdittingCalendar = false;
	};
}
