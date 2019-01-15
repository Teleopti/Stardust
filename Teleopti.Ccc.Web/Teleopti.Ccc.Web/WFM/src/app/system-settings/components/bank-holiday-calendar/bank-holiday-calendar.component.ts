import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NzNotificationService } from 'ng-zorro-antd';
import { TogglesService } from 'src/app/core/services';
import { BankHolidayCalendarItem } from '../../interface';
import { BankCalendarDataService } from '../../shared/bank-calendar-data.service';

@Component({
	selector: 'bank-holiday-calendar',
	templateUrl: './bank-holiday-calendar.component.html',
	styleUrls: ['./bank-holiday-calendar.component.scss']
})
export class BankHolidayCalendarComponent implements OnInit {
	bankHolidayCalendarsList: BankHolidayCalendarItem[] = [];
	isAssignBankHolidayCalendarsToSitesEnabled = false;
	isAddingNewCalendar = false;
	isEdittingCalendar = false;
	edittingCalendar: BankHolidayCalendarItem;
	selectedCalendar: BankHolidayCalendarItem;
	isDeleteCalendarModalVisible = false;
	confirmDeleteCalendarContent: string;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private toggleService: TogglesService
	) {}

	ngOnInit(): void {
		this.bankCalendarDataService.bankHolidayCalendarsList$.subscribe(value => {
			this.bankHolidayCalendarsList = value;
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

		this.toggleService.toggles$.subscribe(toggles => {
			this.isAssignBankHolidayCalendarsToSitesEnabled =
				toggles.WFM_Setting_AssignBankHolidayCalendarsToSites_79899;

			if (this.isAssignBankHolidayCalendarsToSitesEnabled) {
				this.bankCalendarDataService.getSitesByCalendar(calendar.Id).subscribe(result => {
					if (Array.isArray(result)) {
						this.confirmDeleteCalendarContent = this.translate
							.instant('XSitesUseThisCalendar')
							.replace('{0}', result.length);
						this.isDeleteCalendarModalVisible = true;
					} else {
						this.networkError();
					}
				}, this.networkError);
			} else {
				this.confirmDeleteCalendarContent = '';
				this.isDeleteCalendarModalVisible = true;
			}
		});
	}

	deleteHolidayCalendar() {
		this.isDeleteCalendarModalVisible = false;
		this.bankCalendarDataService.deleteBankHolidayCalendar(this.selectedCalendar.Id).subscribe(result => {
			if (result === true) {
				this.bankHolidayCalendarsList.splice(this.bankHolidayCalendarsList.indexOf(this.selectedCalendar), 1);
				this.bankCalendarDataService.bankHolidayCalendarsList$.next(this.bankHolidayCalendarsList);

				this.noticeService.success(
					this.translate.instant('Success'),
					this.translate
						.instant('BankHolidayCalendarHasBeenSuccessfullyDeleted')
						.replace('{0}', this.selectedCalendar.Name)
				);
			}
		}, this.networkError);
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

	backToBankCalendarsList = () => {
		this.isAddingNewCalendar = false;
		this.isEdittingCalendar = false;
	};

	networkError = (error?: any) => {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
