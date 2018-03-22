import { Component, Input, Output, EventEmitter, ViewChild, OnInit } from '@angular/core';
import { Person, Role } from '../../../types';
import { WorkspaceService, RolesService, NavigationService } from '../../../services';
import { SearchOverridesService } from '../../../services/search-overrides.service';

@Component({
	template: '<div>Override me</div>'
})
export class RolePage implements OnInit {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public rolesService: RolesService,
		public searchOverridesService: SearchOverridesService
	) {}

	@ViewChild('saveBtn') saveButton: HTMLElement;
	roles: Array<Role> = [];
	selectedRoles = [];

	ngOnInit() {
		this.workspaceService.getSelectedPeople().subscribe({
			next: (people: Person[]) => {
				if (people.length === 0) {
					this.close();
				}
			}
		});
	}

	getRole(id: string): Role {
		return this.roles.find(role => role.Id === id);
	}

	getRolesOfPeople(): Array<Role> {
		let uniqueRoles = [];
		this.workspaceService
			.getSelectedPeople()
			.getValue()
			.forEach(({ Roles }) => {
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
		this.nav.navToSearch();
	}

	close() {
		this.nav.navToSearch();
	}

	isRoleOnAll(roleId: string): boolean {
		return (
			this.workspaceService
				.getSelectedPeople()
				.getValue()
				.filter(({ Roles }) => Roles.map(role => role.Id).includes(roleId)).length ===
			this.workspaceService.getSelectedPeople().getValue().length
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
		return this.workspaceService.getSelectedPeople().getValue().length > 1;
	}

	isSingleSelected(): boolean {
		return this.workspaceService.getSelectedPeople().getValue().length === 1;
	}
}
