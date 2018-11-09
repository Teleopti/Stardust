import { Component, Input, Output, OnInit } from '@angular/core';
import * as pbi from 'powerbi-client';

import { ReportService } from '../../core/report.service';
import { ReportConfig } from '../../models/ReportConfig.model';

@Component({
	selector: 'app-insights-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	@Input() initialized: boolean;
	public canEditReport = false;
	public reportPermission: pbi.models.Permissions;
	public enableFilter: boolean;
	public enableNavContent: boolean;

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
		this.reportSvc.getReports().then((reports) => {
			// reports.forEach(report => {
			// 	console.log(report.Id, report.Name, report.EmbedUrl);
			// });
			console.log(reports);
		});
		this.loadReport();
	}

	onEmbedded() {
	}

	loadReport() {
		this.reportSvc.getReportConfig('1b44e7ff-7577-4e5f-a615-09c82ff82e13').then((config) => {
			// Refer to https://github.com/Microsoft/PowerBI-JavaScript/wiki/Embed-Configuration-Details for more details
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
			const reportContainer = <HTMLElement>document.getElementById('reportContainer');
			const report = this.pbiCoreService.embed(reportContainer, embedConfig);

			// Report.off removes a given event handler if it exists.
			report.off('loaded');

			// Report.on will add an event handler which prints to Log window.
			report.on('loaded', function() {
				// console.log('Report loaded');
			});

			this.initialized = true;
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
