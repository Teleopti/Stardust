import { Component, Input, OnInit } from '@angular/core';
import { ReportService } from '../../core/report.service';
import { NavigationService } from '../../core/navigation.service';
import { Report } from '../../models/Report.model';

@Component({
	selector: 'app-insights-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	@Input() initialized: boolean;
	@Input() isLoading: boolean;
	@Input() hasViewPermission: boolean;
	@Input() hasEditPermission: boolean;
	public reports: Report[];

	constructor(private reportSvc: ReportService,
		public nav: NavigationService) {
		this.initialized = false;
	}

	ngOnInit() {
		this.reportSvc.getPermission().then(permission => {
			this.hasViewPermission = permission.CanViewReport;
			this.hasEditPermission = permission.CanEditReport;

			if (this.hasViewPermission || this.hasEditPermission) {
				this.loadReportList();
			}

			this.initialized = true;
		});
	}

	loadReportList() {
		this.isLoading = true;
		this.reportSvc.getReports().then(reports => {
			this.reports = [];
			reports.forEach(report => {
				if (report.Name.trim() !== 'Report Usage Metrics Report') {
					this.reports.push(report);
				}
			});

			this.reports = this.reports.sort();
			this.isLoading = false;
		});
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	public cloneReport(report) {
		this.isLoading = true;
		this.reportSvc.cloneReport(report.Id).then(() => {
			this.loadReportList();
		});
	}

	public deleteReport(report) {
		this.isLoading = true;
		this.reportSvc.deleteReport(report.Id).then(deleted => {
			this.isLoading = false;
			if (deleted) {
				this.loadReportList();
			} else {
				console.log('Failed to delete report "' + report.Name + '"');
			}
		});
	}
}
