import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Person, Role } from '../../../types';
import { WorkspaceService, RolesService } from '../../../services';

@Component({
	template: '<div>Override me</div>'
})
export class RolePage {
	constructor(public workspaceService: WorkspaceService, public rolesService: RolesService) {}

	@ViewChild('saveBtn') saveButton: HTMLElement;
	@Output() onRolesChanged = new EventEmitter();
	roles: Array<Role> = [];
	selectedRoles = [];

	getRole(id: string): Role {
		return this.roles.find(role => role.Id === id);
	}

	getRolesOfPeople(): Array<Role> {
		let uniqueRoles = [];
		this.workspaceService.getSelectedPeople().forEach(({ Roles }) => {
			Roles.forEach(role => {
				const isUniqueRole = uniqueRoles.findIndex(r => r.Id === role.Id) === -1;
				if (isUniqueRole) {
					uniqueRoles.push(role);
				}
			});
		});
		return uniqueRoles.sort((role1, role2) => {
			return role1.Name.localeCompare(role2.Name);
		});
	}

	getRolesNotOnAll() {
		return this.roles.filter(role => !this.isRoleOnAll(role.Id));
	}

	toggleSelectedRole(id: string): void {
		if (!this.selectedRoles.includes(id)) {
			this.selectedRoles = [...this.selectedRoles, id];
			//this.focusOnSave();
		} else {
			this.selectedRoles = this.selectedRoles.filter(roleId => id !== roleId);
		}
	}

	save() {
		this.onRolesChanged.emit();
	}

	close() {
		this.onRolesChanged.emit();
	}

	isRoleOnAll(roleId: string): boolean {
		return (
			this.workspaceService.getSelectedPeople().filter(({ Roles }) => Roles.map(role => role.Id).includes(roleId))
				.length === this.workspaceService.getSelectedPeople().length
		);
	}

	isAnyRoleSelected(): boolean {
		return this.selectedRoles.length > 0;
	}

	focusOnSave(): void {
		if (typeof this.saveButton !== 'undefined') {
			this.saveButton.focus();
		}
	}

	isSelectedRole(roleId: string): boolean {
		return this.selectedRoles.includes(roleId);
	}

	isMultipleSelected(): boolean {
		return this.workspaceService.getSelectedPeople().length > 1;
	}

	isSingleSelected(): boolean {
		return this.workspaceService.getSelectedPeople().length === 1;
	}
}
