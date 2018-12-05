import { Component, Input, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ReportService } from '../../core/report.service';
import { NavigationService } from '../../core/navigation.service';
import { Report } from '../../models/Report.model';
import { NzModalService, NzModalRef } from 'ng-zorro-antd';

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
	@Input() reportNameCriteria: string;

	public reports: Report[];
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
			this.hasViewPermission = permission.CanViewReport;
			this.hasEditPermission = permission.CanEditReport;

			if (this.hasViewPermission || this.hasEditPermission) {
				this.loadReportList();
			}

			this.initialized = true;
		});
	}

	loadReportList() {
		this.isLoading = true;
		this.reportSvc.getReports().then(reports => {
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

	getReportContainer() {
		return <HTMLElement>document.getElementById('reportContainer');
	}

	cancelCloneReport(): void {
		this.refNewReportNameModal.destroy();
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

	public confirmCloneReport(report) {
		this.messageForNewReportName = `Please input name for clone of report "${report.Name}":`;
		this.refNewReportNameModal = this.modalSvc.create({
			nzTitle: 'Clone report',
			nzContent: this.newReportNameTempRef,
			nzOnOk: () => this.cloneReport(report),
			nzOnCancel: () => this.cancelCloneReport()
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
