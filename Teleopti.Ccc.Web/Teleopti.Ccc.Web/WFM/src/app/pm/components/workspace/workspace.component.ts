import { Component, Input, OnInit } from '@angular/core';
import { PowerBIService } from '../../core/powerbi.service';

import { ReportConfig } from '../../models/ReportConfig.model';

@Component({
	selector: 'app-pm-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	@Input() initialized: bool;
	@Input() reportConfig: ReportConfig;

	constructor(private pbiService: PowerBIService) {
		this.initialized = false;
	}

	ngOnInit() {
		this.pbiService.getReportConfig().then((config) => {
			this.reportConfig = config;
			this.initialized = true;
		});
	}

	onEmbedded() {
	}
}
