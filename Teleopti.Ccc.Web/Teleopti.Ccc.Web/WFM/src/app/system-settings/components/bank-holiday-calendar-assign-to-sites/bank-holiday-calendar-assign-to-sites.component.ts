import { TranslateService } from '@ngx-translate/core';
import { Component, OnInit, Input } from '@angular/core';
import { NzNotificationService } from 'ng-zorro-antd';

import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { GroupPageSiteItem } from 'src/app/shared/interface';
import { BankHolidayCalendarItem, UpdateSiteBankHolidayCalendarsFormData } from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-assign-to-sites',
	templateUrl: './bank-holiday-calendar-assign-to-sites.component.html',
	styleUrls: ['./bank-holiday-calendar-assign-to-sites.component.scss'],
	providers: [GroupPageService, BankCalendarDataService]
})
export class BankHolidayCalendarAssignToSitesComponent implements OnInit {
	@Input() bankHolidayCalendarsList: BankHolidayCalendarItem[];

	sitesList: GroupPageSiteItem[];

	constructor(
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private groupPageService: GroupPageService,
		private bankCalendarDataService: BankCalendarDataService
	) {}

	ngOnInit(): void {
		this.groupPageService.getAvailableGroupPagesForDate(moment().format('YYYY-MM-DD')).subscribe(result => {
			this.sitesList = result.BusinessHierarchy as GroupPageSiteItem[];

			this.bankCalendarDataService.getSiteBankHolidayCalendars().subscribe(siteCalendars => {
				if (siteCalendars && siteCalendars.length > 0) {
					this.sitesList.forEach(s => {
						let site = siteCalendars.filter(sc => sc.Site == s.Id)[0];
						if (site) {
							s.SelectedCalendarId = site.Calendars[0].Id;
						}
					});
				}
			});
		});
	}

	updateCalendarForSite(calId: string, site: GroupPageSiteItem) {
		let data: UpdateSiteBankHolidayCalendarsFormData = {
			Settings: [{ Site: site.Id, Calendars: [calId] }]
		};
		this.bankCalendarDataService.updateCalendarForSite(data).subscribe(
			result => {
				if (result) {
				} else {
					this.sitesList.forEach(s => {
						if (s.Id == site.Id) {
							s.SelectedCalendarId = '';
						}
					});
					this.updateError();
				}
			},
			error => {
				this.updateError();
			}
		);
	}

	updateError() {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	}
}
