import { Component, Input, Output, EventEmitter, ViewChild} from '@angular/core';
import { Person, Role } from '../../../types';
import { WorkspaceService } from '../../../services';

@Component({
	template: '<div>Override me</div>'
})
export class RolePage{
	constructor(public workspaceService: WorkspaceService) {}
	
	@ViewChild('saveBtn') saveButton: HTMLElement;
	@Input() roles: Array<Role> = [];
	@Output() onRolesChanged = new EventEmitter<Array<Role>>();
	selectedRoles = [];


	getRole(id: string): Role {
		return this.roles.find(role => role.Id === id);
	}

	getRoleIdsOfPeople() {
		let uniqueIds = new Set();
		this.workspaceService.getSelectedPeople().forEach(({ Roles }) => {
			Roles.forEach(role => {
				uniqueIds.add(role.Id);
			});
		});

		return Array.from(uniqueIds).sort((rId1, rId2) => {
			const role1 = this.getRole(rId1);
			const role2 = this.getRole(rId2);
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
		this.onRolesChanged.emit(this.selectedRoles);
	}

	close() {
		this.onRolesChanged.emit([]);
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
		if(typeof this.saveButton !== 'undefined') {
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
