import { Component, Input, Output, OnInit } from '@angular/core';
import * as pbi from 'powerbi-client';

import { ReportService } from '../../core/report.service';
// import { ReportConfig } from '../../models/ReportConfig.model';
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
	public canEditReport = false;
	public reportPermission: pbi.models.Permissions;
	public enableFilter: boolean;
	public enableNavContent: boolean;
	public reports: Report[];
	public selectedReport: string;

	private pbiCoreService: pbi.service.Service;

	constructor(private reportSvc: ReportService) {
		this.initialized = false;

		this.canEditReport = false;
		this.enableFilter = true;
		this.enableNavContent = true;
		this.reportPermission = pbi.models.Permissions.All;

		this.pbiCoreService = new pbi.service.Service(
			pbi.factories.hpmFactory,
			pbi.factories.wpmpFactory,
			pbi.factories.routerFactory
		);
	}

	ngOnInit() {
		this.reportSvc.getPermission().then((permission) => {
			this.hasViewPermission = permission.CanViewReport;
			this.hasEditPermission = permission.CanEditReport;

			if (this.hasViewPermission || this.hasEditPermission) {
				this.loadReportList();
			}

			this.initialized = true;
		});
	}

	onEmbedded() {
	}

	loadReportList() {
		this.isLoading = true;
		this.reportSvc.getReports().then((reports) => {
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

	loadReport(config) {// Refer to https://github.com/Microsoft/PowerBI-JavaScript/wiki/Embed-Configuration-Details for more details
		const embedConfig = {
			type: 'report',
			tokenType: pbi.models.TokenType.Embed,
			accessToken: config.AccessToken,
			embedUrl: config.ReportUrl,
			id: config.ReportId,
			permissions: this.reportPermission,
			viewMode: this.canEditReport ? pbi.models.ViewMode.Edit : pbi.models.ViewMode.View,
			settings: {
				filterPaneEnabled: this.enableFilter,
				navContentPaneEnabled: this.enableNavContent,
				localeSettings: {
					language: 'en',
					formatLocale: 'en'
				}
			}
		};

		// Embed the report and display it within the div container.
		const reportContainer = this.getReportContainer();
		const report = this.pbiCoreService.embed(reportContainer, embedConfig);

		// Report.off removes a given event handler if it exists.
		report.off('loaded');

		// Report.on will add an event handler which prints to Log window.
		report.on('loaded', function() {
			// console.log('Report loaded');
		});

		this.isLoading = true;
	}

	loadSelectedReport(selectedReportId) {
		this.pbiCoreService.reset(this.getReportContainer());
		if (selectedReportId) {
			this.isLoading = true;
			this.reportSvc.getReportConfig(selectedReportId).then((config) => {
				this.loadReport(config);
				this.isLoading = false;
			});
		}
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	public onReportSelected(selectedReportId) {
		this.selectedReport = selectedReportId;
		this.loadSelectedReport(this.selectedReport);
	}

	public reloadCurrentReport() {
		this.loadSelectedReport(this.selectedReport);
	}

	public cloneCurrentReport() {
		this.isLoading = true;
		this.reportSvc.cloneReport(this.selectedReport).then((config) => {
			this.loadReportList();
			this.loadReport(config);
			this.isLoading = false;
		});
	}

	public onCanEditReportChanged() {
		this.canEditReport = !this.canEditReport;
		if (this.canEditReport) {
			this.enableFilter = true;
			this.enableNavContent = true;
		}
	}

	public onEnableFilterChanged() {
		this.enableFilter = !this.enableFilter;
		if (!this.enableNavContent || !this.enableFilter) {
			this.canEditReport = false;
		}
	}

	public onEnableNavContentChanged() {
		this.enableNavContent = !this.enableNavContent;
		if (!this.enableNavContent || !this.enableFilter) {
			this.canEditReport = false;
		}
	}
}
