import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Person, Role } from '../../../types';

@Component({
	template: '<div>Override me</div>'
})
export class RolePage {
	constructor() {}

	@Input() people: Array<Person> = [];
	@Input() roles: Array<Role> = [];
	@Output() onRolesChanged = new EventEmitter<Array<Role>>();
	selectedRoles = [];

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
		this.onRolesChanged.emit(this.selectedRoles);
	}

	close() {
		this.onRolesChanged.emit([]);
	}

	isRoleOnAll(roleId: string): boolean {
		return (
			this.people.filter(({ Roles }) => Roles.map(role => role.Id).includes(roleId)).length === this.people.length
		);
	}

	isAnyRoleSelected(): boolean {
		return this.selectedRoles.length > 0;
	}

	isSelectedRole(roleId: string): boolean {
		return this.selectedRoles.includes(roleId);
	}

	isMultipleSelected(): boolean {
		return this.people.length > 1;
	}

	isSingleSelected(): boolean {
		return this.people.length === 1;
	}
}
