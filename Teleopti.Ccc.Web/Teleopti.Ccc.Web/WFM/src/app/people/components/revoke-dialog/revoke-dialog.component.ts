import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatCheckboxModule } from '@angular/material';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Person, Role } from '../../types';

@Component({
	templateUrl: './revoke-dialog.component.html',
	styleUrls: ['./revoke-dialog.scss']
})
export class RevokeDialog {
	people: Array<Person> = [];
	roles: Array<Role> = [];
	selectedRoles = [];

	constructor(public dialogRef: MatDialogRef<RevokeDialog>, @Inject(MAT_DIALOG_DATA) public data: any) {
		this.people = data.people;
		this.roles = data.roles;
	}

	getRole(id) {
		return this.roles.find(role => role.Id === id);
	}

	getRoleIdsOfPeople() {
		let uniqueIds = new Set();
		this.people.forEach(({ Roles }) => {
			Roles.forEach(role => {
				uniqueIds.add(role.Id);
			});
		});

		return Array.from(uniqueIds);
	}

	getRolesNotOnAll() {
		return this.roles.filter(role => !this.isRoleOnAll(role.Id));
	}

	toggleSelectedRole(id) {
		if (!this.selectedRoles.includes(id)) {
			this.selectedRoles = [...this.selectedRoles, id];
		} else {
			this.selectedRoles = this.selectedRoles.filter(roleId => id !== roleId);
		}
	}

	save() {
		this.dialogRef.close({
			roles: this.selectedRoles
		});
	}

	isRoleOnAll(roleId) {
		return (
			this.people.filter(({ Roles }) => Roles.map(role => role.Id).includes(roleId)).length === this.people.length
		);
	}

	isSelectedRole(roleId) {
		return this.selectedRoles.includes(roleId);
	}

	isMultipleSelected() {
		return this.people.length > 1;
	}

	isSingleSelected() {
		return this.people.length === 1;
	}
}

export interface RevokeResponse {
	roles: Array<string>;
}
