import { Component, OnInit, OnDestroy } from '@angular/core';
import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { NzNotificationService } from 'ng-zorro-antd';

import { Report } from '../../models/Report.model';
import { Permission } from '../../models/Permission.model';
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

	public initialized = false;
	public isLoading = false;
	public isProcessing = false;
	public inEditing = false;

	public isConfirmingClone = false;
	public newReportName: string = undefined;

	public report: Report;
	public permission: Permission;

	constructor(
		@Inject('$state') private $state: IStateService,
		private reportSvc: ReportService,
		public nav: NavigationService,
		private notification: NzNotificationService) {

		const params = $state.params;
		this.report = params.report;
		this.action = params.action;
		this.inEditing = this.action === 'edit';
		this.permission = new Permission();
		this.permission.CanViewReport = true;
		this.permission.CanEditReport = false;
		this.permission.CanDeleteReport = false;

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
		this.reportSvc.getPermission().then(permission => {
			this.permission = permission;

			if (this.permission.CanViewReport) {
				this.reportSvc.getReportConfig(this.report.Id).then(config => {
					this.loadReport(config);
				});
			} else {
				this.isLoading = false;
			}

			this.initialized = true;
		});
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	updateToken(reportId) {
		this.reportSvc.getReportConfig(reportId).then(config => {
			const embedContainer = this.getReportContainer();
			const report = this.pbiCoreService.get(embedContainer);

			const self = this;
			report.setAccessToken(config.AccessToken).then(function () {
				self.setTokenExpirationListener(config.Expiration, 2, reportId);
			});
		});
	}

	setTokenExpirationListener(tokenExpiration, minutesToRefresh, reportId) {
		const currentTime = Date.now();
		const expiration = Date.parse(tokenExpiration);
		const safetyInterval = minutesToRefresh * 60 * 1000;
		const timeout = expiration - currentTime - safetyInterval;

		const self = this;
		if (timeout <= 0) {
			self.updateToken(reportId);
		} else {
			setTimeout(function () {
				self.updateToken(reportId);
			}, timeout);
		}
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

		const reportContainer = this.getReportContainer();
		const report = this.pbiCoreService.embed(reportContainer, embedConfig);

		const self = this;
		report.off('loaded');
		report.on('loaded', function () {
			self.isLoading = false;
			self.setTokenExpirationListener(config.Expiration, 2, config.ReportId);
		});
	}

	public confirmCloneReport() {
		this.isConfirmingClone = true;
	}

	public cancelCloneReport() {
		this.isConfirmingClone = false;
	}

	public cloneReport(report) {
		if (!this.newReportName || this.newReportName.trim().length === 0) return;

		this.isConfirmingClone = false;
		this.isProcessing = true;
		this.reportSvc.cloneReport(report.Id, this.newReportName).then(newReport => {
			this.isProcessing = false;
			this.nav.editReport({
				Id: newReport.ReportId,
				Name: newReport.ReportName,
			});
		});
	}

	public deleteReport(report) {
		this.isProcessing = true;
		this.reportSvc.deleteReport(report.Id).then(deleted => {
			this.isProcessing = false;
			if (deleted) {
				this.nav.gotoInsights();
			} else {
				this.notification.create('error', 'Failed to delete report', 'Failed to delete report "' + report.Name + '"');
			}
		});
	}
}
