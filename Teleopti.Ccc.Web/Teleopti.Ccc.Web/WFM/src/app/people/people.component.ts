import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';
import { Person, Role } from './types';

import { Observable } from 'rxjs/Rx';
import { WorkspaceService, RolesService, SearchService } from './services';

@Component({
	selector: 'app-people',
	templateUrl: './people.component.html',
	styleUrls: ['./people.component.scss']
})
export class PeopleComponent implements OnInit {
	constructor(protected workspaceService: WorkspaceService, public rolesService: RolesService, public searchService: SearchService) { }

	roles: Array<Role> = [];

	ngOnInit() {
		this.searchService.searchPeople().then(people => {
		});
		this.rolesService.getRoles().then(roles => {
			this.roles = roles;
		});
	}

	toggleSelectedPerson(id: string): void {
		const isSelected = this.workspaceService.isPersonSelected(id);
		if (isSelected) {
			const person = this.workspaceService.getSelectedPerson(id)
			this.workspaceService.deselectPerson(person)
		} else {
			const person = this.searchService.getPerson(id)
			this.workspaceService.selectPerson(person)
		}

		// Workspace
		if (this.workspaceService.getSelectedPeople().length === 0) {
			this.displayGrantView = false;
			this.displayRevokeView = false;
		}
	}

	personToRoles(person: Person): string {
		return person.Roles.map(role => role.Name).join(', ');
	}

	getSelectedPeopleCount(): number {
		return this.workspaceService.getSelectedPeople().length;
	}

	grantRoles(roles): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.grantRoles(peopleIds, roles).then(ok => { });
	}

	revokeRoles(roles): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.revokeRoles(peopleIds, roles).then(ok => { });
	}

	/** Below is window grant component */
	displayGrantView = false;
	displayRevokeView = false;
	toggleGrantView(): void {
		this.displayGrantView = !this.displayGrantView;
	}
	toggleRevokeView(): void {
		this.displayRevokeView = !this.displayRevokeView;
	}

	handleGranted(roles: Array<Role>): void {
		if (roles.length > 0) this.grantRoles(roles);
		this.toggleGrantView();
	}

	handleRevoked(roles: Array<Role>): void {
		if (roles.length > 0) this.revokeRoles(roles);
		this.toggleRevokeView();
	}
}
