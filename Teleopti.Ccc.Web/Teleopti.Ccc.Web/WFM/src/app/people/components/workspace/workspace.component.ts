import { Component } from '@angular/core';
import { WorkspaceService } from '../../shared';

@Component({
	selector: 'people-workspace',
	templateUrl: './workspace.component.html',
	styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent {
	constructor(public workspaceService: WorkspaceService) {}
}
