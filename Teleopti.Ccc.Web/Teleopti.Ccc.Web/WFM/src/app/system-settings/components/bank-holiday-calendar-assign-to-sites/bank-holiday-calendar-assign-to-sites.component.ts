import { TranslateService } from '@ngx-translate/core';
import { Component, OnInit, Input } from '@angular/core';
import { NzNotificationService } from 'ng-zorro-antd';

import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { GroupPageSiteItem } from 'src/app/shared/interface';
import { BankHolidayCalendarItem } from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-assign-to-sites',
	templateUrl: './bank-holiday-calendar-assign-to-sites.component.html',
	styleUrls: ['./bank-holiday-calendar-assign-to-sites.component.scss'],
	providers: [GroupPageService, BankCalendarDataService]
})
export class BankHolidayCalendarAssignToSitesComponent implements OnInit {
	@Input() bankHolidayCalendarsList: BankHolidayCalendarItem[];

	dateFormat = 'YYYY-MM-DD';
	sitesList: GroupPageSiteItem[] = [];

	constructor(
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private groupPageService: GroupPageService,
		private bankCalendarDataService: BankCalendarDataService
	) {}

	ngOnInit() {
		this.groupPageService.getAvailableGroupPagesForDate(moment().format('YYYY-MM-DD')).subscribe(result => {
			this.sitesList = result.BusinessHierarchy as GroupPageSiteItem[];

			this.bankCalendarDataService.getSiteBankHolidayCalendars().subscribe(siteCalendars => {
				this.sitesList.forEach(s => {
					s.SelectedCalendarId = null;
					const site = siteCalendars.filter(sc => sc.Site === s.Id)[0];
					if (site && site.Calendars[0] && site.Calendars[0].length > 0) {
						// Currently we only support setting one calendar to site
						s.SelectedCalendarId = site.Calendars[0];
					}
				});
			}, this.networkError);
		}, this.networkError);
	}

	updateCalendarForSite(calId: string, site: GroupPageSiteItem) {
		const cals: string[] = [];

		if (calId && calId.length > 0) {
			cals.push(calId);
		}

		const formData = {
			Settings: [{ Site: site.Id, Calendars: cals }]
		};

		this.bankCalendarDataService.updateCalendarForSite(formData).subscribe(
			result => {
				if (result) {
				} else {
					site.SelectedCalendarId = null;
					this.networkError();
				}
			},
			error => {
				site.SelectedCalendarId = null;
				this.networkError(error);
			}
		);
	}

	networkError(error?: any) {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	}
}
