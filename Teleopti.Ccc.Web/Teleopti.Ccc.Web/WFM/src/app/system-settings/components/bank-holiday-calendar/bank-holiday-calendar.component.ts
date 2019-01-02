import { Component, OnInit } from '@angular/core';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';
import { BankHolidayCalendar } from '../../interface';
import { NzNotificationService } from 'ng-zorro-antd';
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
	selectedCalendar: BankHolidayCalendar;
	isDeleteCalendarModalVisible: boolean = false;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
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
		this.selectedCalendar = calendar;
		this.isDeleteCalendarModalVisible = true;
	}

	deleteHolidayCalendar() {
		this.isDeleteCalendarModalVisible = false;
		this.bankCalendarDataService.deleteBankHolidayCalendar(this.selectedCalendar.Id).subscribe(
			result => {
				if (result === true) {
					this.bankHolidayCalendarsList.splice(
						this.bankHolidayCalendarsList.indexOf(this.selectedCalendar),
						1
					);

					this.noticeService.success(
						this.translate.instant('Success'),
						this.translate
							.instant('BankHolidayCalendarHasBeenSuccessfullyDeleted')
							.replace('{0}', this.selectedCalendar.Name)
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

	closeDeleteHolidayCalendarModal() {
		this.isDeleteCalendarModalVisible = false;
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
