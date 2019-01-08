import { Component, OnInit, Input } from '@angular/core';
import { GroupPageService } from 'src/app/shared/services/group-page-service';
import { GroupPageSite } from 'src/app/shared/interface';

@Component({
	selector: 'bank-holiday-calendar-assign-to-sites',
	templateUrl: './bank-holiday-calendar-assign-to-sites.component.html',
	styleUrls: ['./bank-holiday-calendar-assign-to-sites.component.scss'],
	providers: [GroupPageService]
})
export class BankHolidayCalendarAssignToSitesComponent implements OnInit {
	sitesList: GroupPageSite[];
	constructor(private groupPageService: GroupPageService) {}

	ngOnInit(): void {
		this.groupPageService.getAvailableGroupPagesForDate(moment().format('YYYY-MM-DD')).subscribe(result => {
			this.sitesList = result.BusinessHierarchy;
		});
	}
}
