import { Component, Input, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ReportService } from '../../core/report.service';
import { NavigationService } from '../../core/navigation.service';
import { Report } from '../../models/Report.model';
import { Permission } from '../../models/Permission.model';
import { NzModalService, NzModalRef } from 'ng-zorro-antd';

@Component({
	selector: 'app-insights-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	public initialized: boolean;
	public isLoading: boolean;
	public reportNameCriteria: string;

	public reports: Report[];
	public permission: Permission;
	public newReportName: string = undefined;
	public messageForNewReportName = '';
	public refNewReportNameModal: NzModalRef;

	@ViewChild('newReportNameTemplate')
	private newReportNameTempRef: TemplateRef<any>;

	constructor(private reportSvc: ReportService,
		private modalSvc: NzModalService,
		public nav: NavigationService) {
		this.initialized = false;
	}

	ngOnInit() {
		this.reportSvc.getPermission().then(permission => {
			this.permission = permission;

			if (this.permission.CanViewReport) {
				this.loadReportList();
			}

			this.initialized = true;
		});
	}

	loadReportList() {
		this.isLoading = true;
		this.reportSvc.getReports().then(reports => {
			this.reports = reports.sort();
			this.isLoading = false;
		});
	}

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	cancelCreateReport(): void {
		this.refNewReportNameModal.destroy();
		this.newReportName = undefined;
	}

	createReport(): boolean {
		if (!this.newReportName || this.newReportName.trim().length === 0) {
			return false;
		}

		if (this.refNewReportNameModal !== undefined) {
			this.refNewReportNameModal.destroy();
		}

		this.isLoading = true;
		this.reportSvc.createReport(this.newReportName).then((newReport) => {
			this.nav.editReport({
				Id: newReport.ReportId,
				Name: newReport.ReportName,
			});
			return true;
		});

		this.newReportName = undefined;
	}

	cloneReport(report): boolean {
		if (!this.newReportName || this.newReportName.trim().length === 0) {
			return false;
		}

		if (this.refNewReportNameModal !== undefined) {
			this.refNewReportNameModal.destroy();
		}

		this.isLoading = true;
		this.reportSvc.cloneReport(report.Id, this.newReportName).then(() => {
			this.loadReportList();
			return true;
		});

		this.newReportName = undefined;
	}

	public confirmCreateReport(report) {
		this.messageForNewReportName = 'Please input name for new report:';
		this.refNewReportNameModal = this.modalSvc.create({
			nzTitle: 'Create new report',
			nzContent: this.newReportNameTempRef,
			nzOnOk: () => this.createReport(),
			nzOnCancel: () => this.cancelCreateReport()
		});
	}

	public confirmCloneReport(report) {
		this.messageForNewReportName = `Please input name for new copy of report "${report.Name}":`;
		this.refNewReportNameModal = this.modalSvc.create({
			nzTitle: 'Save as new report',
			nzContent: this.newReportNameTempRef,
			nzOnOk: () => this.cloneReport(report),
			nzOnCancel: () => this.cancelCreateReport()
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
