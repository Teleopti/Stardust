import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NzModalRef, NzModalService } from 'ng-zorro-antd';
import { NavigationService } from '../../core/navigation.service';
import { ReportService } from '../../core/report.service';
import { Permission } from '../../models/Permission.model';
import { Report } from '../../models/Report.model';

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
	public modalTitle = '';
	public refNewReportNameModal: NzModalRef;

	@ViewChild('newReportNameTemplate')
	private newReportNameTempRef: TemplateRef<any>;
	modalIsVisible = false;
	private modalType = '';
	modalReport: Report;

	constructor(private reportSvc: ReportService, private modalSvc: NzModalService, public nav: NavigationService) {
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

	handleOk(): void {
		if (!this.isValidReportName(this.newReportName)) {
			return;
		}

		if (this.modalType === 'create') {
			this.createReport();
		} else if (this.modalType === 'clone') {
			this.cloneReport(this.modalReport);
		}

		this.modalIsVisible = false;
	}

	handleCancel(): void {
		this.modalIsVisible = false;
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
		if (!this.isValidReportName(this.newReportName)) {
			return false;
		}

		if (this.refNewReportNameModal !== undefined) {
			this.refNewReportNameModal.destroy();
		}

		this.isLoading = true;
		this.reportSvc.createReport(this.newReportName).then(newReport => {
			this.nav.editReport({
				Id: newReport.ReportId,
				Name: newReport.ReportName
			});
			return true;
		});

		this.newReportName = undefined;
	}

	cloneReport(report): boolean {
		if (!this.isValidReportName(this.newReportName)) {
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

	isValidReportName(reportName): boolean {
		return reportName && reportName.trim().length > 0;
	}

	public confirmCreateReport() {
		this.modalType = 'create';
		this.modalIsVisible = true;
		this.messageForNewReportName = 'Please input name for new report:';
		this.modalTitle = 'Create new report';
	}

	public confirmCloneReport(report) {
		this.modalType = 'clone';
		this.messageForNewReportName = `Please input name for new copy of report "${report.Name}":`;
		this.modalIsVisible = true;
		this.modalTitle = 'Save as new report';
		this.modalReport = report;
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
