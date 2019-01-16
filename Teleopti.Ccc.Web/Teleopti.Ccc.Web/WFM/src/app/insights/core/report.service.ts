import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NzNotificationService } from 'ng-zorro-antd';

import { ReportConfig } from '../models/ReportConfig.model';
import { Report } from '../models/Report.model';
import { Permission } from '../models/Permission.model';

@Injectable()
export class ReportService {
	constructor(private http: HttpClient,
		private notification: NzNotificationService) { }

	private url_getReportConfig = '../api/Insights/ReportConfig';
	private url_getReports = '../api/Insights/Reports';
	private url_getPermissions = '../api/Insights/Permission';
	private url_createReport = '../api/Insights/CreateReport';
	private url_cloneReport = '../api/Insights/CloneReport';
	private url_deleteReport = '../api/Insights/DeleteReport';

	async getReportConfig(reportId: string): Promise<ReportConfig> {
		const parameters = {
			params: {
				reportId: reportId
			}
		};

		return this.http.get(this.url_getReportConfig, parameters)
			.toPromise()
			.catch(this.handleError);
	}

	async createReport(newReportName: string): Promise<ReportConfig> {
		const parameters = {
			params: {
				newReportName: newReportName
			}
		};

		return this.http.get(this.url_createReport, parameters)
			.toPromise()
			.catch(this.handleError);
	}

	async cloneReport(reportId: string, newReportName: string): Promise<ReportConfig> {
		const parameters = {
			params: {
				reportId: reportId,
				newReportName: newReportName
			}
		};

		return this.http.get(this.url_cloneReport, parameters)
			.toPromise()
			.catch(this.handleError);
	}

	async getReports(): Promise<Report[]> {
		return this.http.get(this.url_getReports)
			.toPromise()
			.catch(this.handleError);
	}

	async deleteReport(reportId: string): Promise<boolean> {
		const parameters = {
			params: {
				reportId: reportId
			}
		};

		return this.http.get(this.url_deleteReport, parameters)
			.toPromise()
			.catch(this.handleError);
	}

	async getPermission(): Promise<Permission> {
		return this.http.get(this.url_getPermissions)
			.toPromise()
			.catch(this.handleError);
	}

	private handleError(error: any): Promise<any> {
		this.notification.create('error', 'Web access failed', `Error message: "${error.messsage}"`);
		return Promise.reject(error.message || error);
	}
}
