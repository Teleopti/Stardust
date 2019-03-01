import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { NzNotificationService, NzModalService } from 'ng-zorro-antd';
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
	isEdittingCalendar = false;
	selectedCalendar: BankHolidayCalendarItem;

	constructor(
		private bankCalendarDataService: BankCalendarDataService,
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private toggleService: TogglesService,
		private modalService: NzModalService
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
		calendar.ActiveYearIndex = event.index;
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
						this.modalService.confirm({
							nzTitle: this.translate.instant('AreYouSureYouWantToDeleteThisBankHolidayCalendar'),
							nzClassName: 'bank-holiday-calendar-modal',
							nzContent:
								'<p><span>' +
								this.translate.instant('XSitesUseThisCalendar').replace('{0}', result.length) +
								'</span><span> </span><span class="calendar-name">' +
								this.selectedCalendar.Name +
								'</span></p>',
							nzOkType: 'danger',
							nzOkText: this.translate.instant('Delete'),
							nzCancelText: this.translate.instant('Cancel'),
							nzOnOk: () => {
								this.deleteHolidayCalendar();
								return true;
							},
							nzOnCancel: () => {}
						});
					} else {
						this.networkError();
					}
				}, this.networkError);
			} else {
				this.modalService.confirm({
					nzTitle: this.translate.instant('AreYouSureYouWantToDeleteThisBankHolidayCalendar'),
					nzClassName: 'bank-holiday-calendar-modal',
					nzContent: '<p><span class="calendar-name">' + this.selectedCalendar.Name + '</span></p>',
					nzOkType: 'danger',
					nzOkText: this.translate.instant('Delete'),
					nzCancelText: this.translate.instant('Cancel'),
					nzOnOk: () => {
						this.deleteHolidayCalendar();
						return true;
					},
					nzOnCancel: () => {}
				});
			}
		});
	}

	deleteHolidayCalendar() {
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

	startAddNewBankCalender() {
		this.isEdittingCalendar = true;
		this.selectedCalendar = null;
	}

	exitAddNewBankCalendar = () => {
		this.isEdittingCalendar = false;
	};

	startEditBankCalendar(event: Event, calendar: BankHolidayCalendarItem) {
		event.stopPropagation();

		this.selectedCalendar = calendar;
		this.isEdittingCalendar = true;
	}

	backToBankCalendarsList = () => {
		this.isEdittingCalendar = false;
	};

	networkError = (error?: any) => {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
