import { Component, Input, OnInit } from '@angular/core';
import * as pbi from 'powerbi-client';

import { PowerBIService } from '../../core/powerbi.service';
import { ReportConfig } from '../../models/ReportConfig.model';

@Component({
	selector: 'app-pm-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	public initialized: boolean;
	public reportConfig: ReportConfig;

	private pbiCoreService: pbi.service.Service;

	constructor(private pbiService: PowerBIService) {
		this.initialized = false;
		this.pbiCoreService = new pbi.service.Service(
			pbi.factories.hpmFactory,
			pbi.factories.wpmpFactory,
			pbi.factories.routerFactory
		);
	}

	ngOnInit() {
		this.pbiService.getReportConfig().then((config) => {
			// Refer to https://github.com/Microsoft/PowerBI-JavaScript/wiki/Embed-Configuration-Details for more details
			const embedConfig = {
				type: 'report',
				tokenType: pbi.models.TokenType.Embed,
				accessToken: config.AccessToken,
				embedUrl: config.ReportUrl,
				id: config.ReportId,
				permissions: pbi.models.Permissions.All,
				viewMode: pbi.models.ViewMode.Edit,
				settings: {
					filterPaneEnabled: true,
					navContentPaneEnabled: true,
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
				console.log('Report loaded :-)');
			});

			this.initialized = true;
		});
	}

	onEmbedded() {
	}
}
