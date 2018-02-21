import { Component, Inject, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material';
import { Person, Role } from './types';

import { GrantDialog, GrantResponse, RevokeDialog, RevokeResponse, RoleResponse } from './components';
import { Observable } from 'rxjs/Rx';
import { PeopleService, RolesService } from './services';

@Component({
	selector: 'app-people',
	templateUrl: './people.component.html',
	styleUrls: ['./people.component.scss']
})
export class PeopleComponent implements OnInit {
	constructor(private dialog: MatDialog, private peopleService: PeopleService, private rolesService: RolesService) {}

	people: Array<Person> = [];
	roles: Array<Role> = [];
	selectedPeopleIds: Array<string> = [];

	isPeopleSelected() {
		return this.selectedPeopleIds.length > 0;
	}

	getSelectedPeople() {
		return this.people.filter(person => this.isPersonSelected(person.Id));
	}

	toggleSelectedPerson(id) {
		if (!this.selectedPeopleIds.includes(id)) {
			this.selectedPeopleIds = [...this.selectedPeopleIds, id];
		} else {
			this.selectedPeopleIds = this.selectedPeopleIds.filter(personId => personId !== id);
		}

		// Workspace
		if (this.selectedPeopleIds.length === 0) {
			this.displayGrantView = false;
			this.displayRevokeView = false;
		}
	}

	personToRoles(person: Person): string {
		return person.Roles.map(role => role.Name).join(', ');
	}

	isPersonSelected(id) {
		return this.selectedPeopleIds.includes(id);
	}

	getSelectedPeopleCount() {
		return this.selectedPeopleIds.length;
	}

	openDialog(dialogType) {
		return this.dialog.open(dialogType, {
			width: '70vw',
			maxHeight: '80vh',
			panelClass: 'dialog-without-padding',
			data: {
				people: this.getSelectedPeople(),
				roles: this.roles
			}
		});
	}

	openGrantDialog() {
		const dialogRef = this.openDialog(GrantDialog);
		dialogRef.afterClosed().subscribe((result: RoleResponse) => {
			if (typeof result !== 'undefined' && Array.isArray(result.roles)) {
				this.grantRoles(result.roles);
			}
		});
	}

	openRevokeDialog() {
		const dialogRef = this.openDialog(RevokeDialog);
		dialogRef.afterClosed().subscribe((result: RevokeResponse) => {
			if (typeof result !== 'undefined' && Array.isArray(result.roles)) {
				this.revokeRoles(result.roles);
			}
		});
	}

	grantRoles(roles) {
		this.rolesService.grantRoles(this.selectedPeopleIds, roles).then(ok => {});
	}

	revokeRoles(roles) {
		this.rolesService.revokeRoles(this.selectedPeopleIds, roles).then(ok => {});
	}

	ngOnInit() {
		this.rolesService.getPeople().then(people => {
			this.people = people;
		});
		this.rolesService.getRoles().then(roles => {
			this.roles = roles;
		});
	}

	/** Below is window grant component */
	displayGrantView = false;
	displayRevokeView = false;
	toggleGrantView() {
		this.displayGrantView = !this.displayGrantView;
	}
	toggleRevokeView() {
		this.displayRevokeView = !this.displayRevokeView;
	}

	handleGranted(roles: Array<Role>) {
		if (roles.length > 0) this.grantRoles(roles);
		this.toggleGrantView();
	}

	handleRevoked(roles: Array<Role>) {
		if (roles.length > 0) this.revokeRoles(roles);
		this.toggleRevokeView();
	}
}
