import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { ReportConfig } from '../models/ReportConfig.model';
import { Report } from '../models/Report.model';
import { Permission } from '../models/Permission.model';

@Injectable()
export class ReportService {
	constructor(private http: HttpClient) { }

	private url_getReportConfig = '../api/Insights/ReportConfig';
	private url_getReports = '../api/Insights/Reports';
	private url_getPermissions = '../api/Insights/Permission';
	private url_cloneReport = '../api/Insights/CloneReport';

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

	async cloneReport(reportId: string): Promise<ReportConfig> {
		const parameters = {
			params: {
				reportId: reportId
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

	async getPermission(): Promise<Permission> {
		return this.http.get(this.url_getPermissions)
			.toPromise()
			.catch(this.handleError);
	}

	private handleError(error: any): Promise<any> {
		console.error('An error occurred', error);
		return Promise.reject(error.message || error);
	}
}
