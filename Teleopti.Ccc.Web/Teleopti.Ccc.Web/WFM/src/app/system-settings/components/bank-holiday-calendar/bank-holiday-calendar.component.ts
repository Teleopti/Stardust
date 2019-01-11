import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NzNotificationService } from 'ng-zorro-antd';
import { TogglesService } from 'src/app/core/services';
import { BankHolidayCalendarItem } from '../../interface';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';

@Component({
	selector: 'bank-holiday-calendar',
	templateUrl: './bank-holiday-calendar.component.html',
	styleUrls: ['./bank-holiday-calendar.component.scss'],
	providers: [BankCalendarDataService, TogglesService]
})
export class BankHolidayCalendarComponent implements OnInit {
	yearFormat = 'YYYY';
	dateFormat = 'YYYY-MM-DD';

	bankHolidayCalendarsList: BankHolidayCalendarItem[] = [];
	isAddingNewCalendar = false;
	isEdittingCalendar = false;
	edittingCalendar: BankHolidayCalendarItem;
	selectedCalendar: BankHolidayCalendarItem;
	isDeleteCalendarModalVisible = false;
	isAssignBankHolidayCalendarsToSitesEnabled = false;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private toggleService: TogglesService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.getBankHolidayCalendars().subscribe(calendars => {
			const cals = calendars as BankHolidayCalendarItem[];
			const curYear = moment().year();
			cals.forEach(c => {
				c.CurrentYearIndex = 0;

				c.Years.forEach((y, i) => {
					y.Dates.forEach(d => {
						d.Date = moment(d.Date).format(this.dateFormat);
					});

					if (moment(y.Year.toString()).year() === curYear) {
						c.CurrentYearIndex = i;
					}
				});
			});

			this.bankHolidayCalendarsList = cals.sort((c, n) => {
				return c.Name.localeCompare(n.Name);
			});
		});

		this.toggleService.toggles$.subscribe(toggles => {
			this.isAssignBankHolidayCalendarsToSitesEnabled =
				toggles.WFM_Setting_AssignBankHolidayCalendarsToSites_79899;
		});
	}

	selectedTabChange(event, calendar: BankHolidayCalendarItem) {
		calendar.CurrentYearIndex = event.index;
	}

	confirmDeleteHolidayCanlendar(event: Event, calendar: BankHolidayCalendarItem) {
		event.stopPropagation();
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

	startEditBankCalendar(event: Event, calendar: BankHolidayCalendarItem) {
		event.stopPropagation();

		this.edittingCalendar = calendar;
		this.isEdittingCalendar = true;
	}

	exitEdittingBankCalendar = () => {
		this.isEdittingCalendar = false;
	};
}
