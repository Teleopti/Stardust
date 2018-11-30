import { Injectable } from '@angular/core';
import { NavigationService as NavigationServiceCore } from './../../core/services/navigation.service';

@Injectable({
	providedIn: 'root'
})
export class NavigationService {
	constructor(private navService: NavigationServiceCore) {}

	public viewReport(report) {
		this.openReport(report, 'view');
	}

	public editReport(report) {
		this.openReport(report, 'edit');
	}

	openReport(report, action) {
		const parameters = {
			report: report,
			action: action
		};
		this.navService.go('insights.report', parameters);
	}
}
