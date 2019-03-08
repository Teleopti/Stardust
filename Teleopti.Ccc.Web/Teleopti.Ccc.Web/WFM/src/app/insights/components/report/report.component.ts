import { Component, OnInit } from '@angular/core';
import { Inject } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { NzNotificationService } from 'ng-zorro-antd';
import { TranslateService } from '@ngx-translate/core';

import { Permission } from '../../models/Permission.model';
import { ReportService } from '../../core/report.service';
import { NavigationService } from '../../core/navigation.service';
import { IPowerBiElement } from 'service';

@Component({
	selector: 'app-report',
	templateUrl: './report.component.html',
	styleUrls: ['./report.component.scss']
})
export class ReportComponent implements OnInit {
	private pbiCoreService: any;
	private action = this.nav.viewAction;

	private errorNotificationOption = { nzDuration: 0 };

	public initialized = false;
	public reportLoaded = false;
	public isProcessing = false;
	public inEditing = false;

	public isConfirmingClone = false;
	public newReportName: string = undefined;

	public reportId: string;
	public reportName: string;
	public permission: Permission;

	constructor(
		@Inject('$state') private $state: IStateService,
		private translate: TranslateService,
		private reportSvc: ReportService,
		public nav: NavigationService,
		private notification: NzNotificationService) {

		const params = $state.params;
		this.reportId = params.reportId.trim();
		this.action = params.action.trim().toLowerCase();

		this.inEditing = this.action === this.nav.editAction;
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
		if (!this.reportId) {
			return;
		}

		this.reportLoaded = false;
		this.reportSvc.getPermission().then(permission => {
			this.permission = permission;

			if (this.action === this.nav.editAction && !this.permission.CanEditReport) {
				this.nav.viewReport(this.reportId);
				return;
			}

			if (this.permission.CanViewReport) {
				this.reportSvc.getReportConfig(this.reportId).then(config => {
					this.loadReport(config);
				});
			} else {
				this.reportLoaded = true;
			}

			this.initialized = true;
		}).catch(error => {
			const title = this.translate.instant('FailedToOpenReport');
			const content = this.translate.instant('ErrorMessageWithParameter', {message: error});
			this.notification.create('error', title, content, this.errorNotificationOption);
		});
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	updateToken(reportId) {
		this.reportSvc.getReportConfig(reportId).then(config => {
			const embedContainer = this.getReportContainer();
			if (!embedContainer) return;

			// If redirect to other page on update token, since the report container will be destroyed,
			// the container will not be a embed report, it will failed to get the embed container and cause problem.
			// Refer to test issue #80713: Error when edit a report
			const pbiElement = <IPowerBiElement>embedContainer;
			if (!pbiElement || !pbiElement.powerBiEmbed) return;

			const self = this;
			const report = this.pbiCoreService.get(embedContainer);
			report.setAccessToken(config.AccessToken).then(function () {
				self.setTokenExpirationListener(config.Expiration, 2, reportId);
			});
		}).catch(error => {
			const title = this.translate.instant('FailedToUpdateToken');
			const content = this.translate.instant('ErrorMessageWithParameter', {message: error});
			this.notification.create('error', title, content, this.errorNotificationOption);
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
		this.reportName = config.ReportName;

		// Refer to https://github.com/Microsoft/PowerBI-JavaScript/wiki/Embed-Configuration-Details for more details
		const embedConfig = {
			type: 'report',
			tokenType: pbi.models.TokenType.Embed,
			accessToken: config.AccessToken,
			embedUrl: config.ReportUrl,
			id: config.ReportId,
			permissions: pbi.models.Permissions.All,
			viewMode: this.action === this.nav.editAction
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
			self.reportLoaded = true;
			self.setTokenExpirationListener(config.Expiration, 2, config.ReportId);
		});

		report.off('saved');
		report.on('saved', function () {
			self.reportSvc.updateReport(config.ReportId, config.ReportName);
		});
	}

	public confirmCloneReport() {
		this.isConfirmingClone = true;
	}

	public cancelCloneReport() {
		this.isConfirmingClone = false;
	}

	public printReport() {
		const report = this.pbiCoreService.get(this.getReportContainer());
		report.print().catch(error => {
			const title = this.translate.instant('FailedToPrintReport');
			const content = this.translate.instant('ErrorMessageWithParameter', {message: error});
			this.notification.create('error', title, content, this.errorNotificationOption);
		});
	}

	public cloneReport(reportId) {
		if (!this.newReportName || this.newReportName.trim().length === 0) return;

		this.isConfirmingClone = false;
		this.isProcessing = true;
		this.reportSvc.cloneReport(reportId, this.newReportName).then(newReport => {
			this.isProcessing = false;
			this.nav.editReport(newReport.ReportId);
		}).catch(error => {
			const title = this.translate.instant('FailedToSaveAsNewReport');
			const content = this.translate.instant('ErrorMessageWithParameter', {message: error});
			this.notification.create('error', title, content, this.errorNotificationOption);
		});
	}

	public deleteReport(reportId) {
		let errorMessage;
		this.isProcessing = true;
		this.reportSvc.deleteReport(reportId).then(deleted => {
			this.isProcessing = false;
			if (deleted) {
				this.nav.gotoInsights();
			} else {
				errorMessage = this.translate.instant('FailedToDeleteReportWithReportName', {repName: this.reportName});
			}
		}).catch(error => {
			errorMessage = this.translate.instant('ErrorMessageWithParameter', {message: error});
		});

		if (errorMessage && errorMessage.length > 0) {
			const title = this.translate.instant('FailedToDeleteReport');
			this.notification.create('error', title, errorMessage, this.errorNotificationOption);
		}
	}
}
