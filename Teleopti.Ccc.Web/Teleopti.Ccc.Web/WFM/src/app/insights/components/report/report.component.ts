import { Component, OnInit, OnDestroy } from '@angular/core';
import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';

import { Report } from '../../models/Report.model';
import { ReportService } from '../../core/report.service';
import { NavigationService } from '../../core/navigation.service';

@Component({
	selector: 'app-report',
	templateUrl: './report.component.html',
	styleUrls: ['./report.component.scss']
})
export class ReportComponent implements OnInit {
	private pbiCoreService: any;

	private action = 'view';

	public isLoading = false;
	public report: Report;

	constructor(
		@Inject('$state') private $state: IStateService,
		private reportSvc: ReportService,
		public nav: NavigationService) {

		const params = $state.params;
		this.report = params.report;
		this.action = params.action;

		this.pbiCoreService = new pbi.service.Service(
			pbi.factories.hpmFactory,
			pbi.factories.wpmpFactory,
			pbi.factories.routerFactory
		);
	}

	ngOnInit() {
		if (!this.report) {
			return;
		}

		this.isLoading = true;
		this.reportSvc.getReportConfig(this.report.Id).then(config => {
			this.loadReport(config);
		});
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	loadReport(config) {
		// Refer to https://github.com/Microsoft/PowerBI-JavaScript/wiki/Embed-Configuration-Details for more details
		const embedConfig = {
			type: 'report',
			tokenType: pbi.models.TokenType.Embed,
			accessToken: config.AccessToken,
			embedUrl: config.ReportUrl,
			id: config.ReportId,
			permissions: pbi.models.Permissions.All,
			viewMode: this.action === 'edit'
				? pbi.models.ViewMode.Edit
				: pbi.models.ViewMode.View,
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
		const reportContainer = this.getReportContainer();
		const report = this.pbiCoreService.embed(reportContainer, embedConfig);

		// Report.off removes a given event handler if it exists.
		report.off('loaded');

		// Report.on will add an event handler which prints to Log window.
		report.on('loaded', function() {
			this.isLoading = false;
		});
	}
}
