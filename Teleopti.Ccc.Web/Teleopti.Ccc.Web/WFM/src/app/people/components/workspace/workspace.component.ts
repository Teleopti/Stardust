import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '../../services';

@Component({
	selector: 'people-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
	constructor(public workspaceService: WorkspaceService) {}

	ngOnInit() {}

	getSelectedPeopleCount(): number {
		return this.workspaceService.getSelectedPeople().getValue().length;
	}
}
