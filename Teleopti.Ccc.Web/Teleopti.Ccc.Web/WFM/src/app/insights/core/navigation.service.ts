import { Injectable } from '@angular/core';
import { NavigationService as NavigationServiceCore } from './../../core/services/navigation.service';

@Injectable({
	providedIn: 'root'
})
export class NavigationService {
	constructor(private navService: NavigationServiceCore) {}

	public viewReport(reportId) {
		this.openReport(reportId, 'view');
	}

	public editReport(reportId) {
		this.openReport(reportId, 'edit');
	}

	public gotoInsights() {
		this.navService.go('insights');
	}

	openReport(reportId, action) {
		const parameters = {
			reportId: reportId,
			action: action
		};
		this.navService.go('insights.report', parameters);
	}
}
