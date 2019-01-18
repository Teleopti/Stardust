import { TranslateService } from '@ngx-translate/core';
import { Component, OnInit } from '@angular/core';
import { NzNotificationService } from 'ng-zorro-antd';

import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { GroupPageSiteItem } from 'src/app/shared/interface';
import { BankHolidayCalendarItem, SiteBankHolidayCalendars } from '../../interface';
import { BankCalendarDataService } from '../../shared';

@Component({
	selector: 'bank-holiday-calendar-assign-to-sites',
	templateUrl: './bank-holiday-calendar-assign-to-sites.component.html',
	styleUrls: ['./bank-holiday-calendar-assign-to-sites.component.scss']
})
export class BankHolidayCalendarAssignToSitesComponent implements OnInit {
	bankHolidayCalendarsList: BankHolidayCalendarItem[] = [];
	siteCalendarsList: SiteBankHolidayCalendars[] = [];
	sitesList: GroupPageSiteItem[] = [];

	constructor(
		private translate: TranslateService,
		private noticeService: NzNotificationService,
		private groupPageService: GroupPageService,
		private bankCalendarDataService: BankCalendarDataService
	) {}

	ngOnInit() {
		this.bankCalendarDataService.bankHolidayCalendarsList$.subscribe(calendars => {
			this.bankHolidayCalendarsList = calendars;
			this.updateSiteAndCalsList(calendars);
			this.preSelectCalendarsForSite();
		});

		this.groupPageService
			.getAvailableGroupPagesForDate(moment().format(this.bankCalendarDataService.dateFormat))
			.subscribe(result => {
				this.sitesList = result.BusinessHierarchy as GroupPageSiteItem[];

				this.bankCalendarDataService.getSiteBankHolidayCalendars().subscribe(siteCalendars => {
					this.siteCalendarsList = siteCalendars;

					this.preSelectCalendarsForSite();
				}, this.networkError);
			}, this.networkError);
	}

	updateSiteAndCalsList(calendars: BankHolidayCalendarItem[]) {
		const calIds = calendars.map(c => {
			return c.Id;
		});

		this.siteCalendarsList.forEach(sc => {
			const cals = [];
			sc.Calendars.forEach(c => {
				if (calIds.indexOf(c) > -1) cals.push(c);
			});
			sc.Calendars = cals;
		});
	}

	preSelectCalendarsForSite() {
		this.sitesList.forEach(s => {
			s.SelectedCalendarId = null;
			const site = this.siteCalendarsList.filter(sc => sc.Site === s.Id)[0];
			if (site && site.Calendars[0] && site.Calendars[0].length > 0) {
				// Currently we only support setting one calendar to site
				s.SelectedCalendarId = site.Calendars[0];
			}
		});
		this.sitesList = [...this.sitesList];
	}

	updateCalendarForSite(calId: string, site: GroupPageSiteItem) {
		const cals: string[] = [];
		if (calId && calId.length > 0) {
			cals.push(calId);
		}
		const item = { Site: site.Id, Calendars: cals };

		this.bankCalendarDataService.updateCalendarForSite({ Settings: [item] }).subscribe(
			result => {
				if (result === true) {
					this.siteCalendarsList.forEach(sc => {
						if (sc.Site === item.Site) sc.Calendars = item.Calendars;
					});
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

	networkError = (error?: any) => {
		this.noticeService.error(
			this.translate.instant('Error'),
			this.translate.instant('AnErrorOccurredPleaseCheckTheNetworkConnectionAndTryAgain')
		);
	};
}
