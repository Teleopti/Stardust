import { Component, Inject, OnInit } from '@angular/core';
import { Person, Role } from '../../types';
import { RolePage } from '../shared/role-page';

@Component({
	selector: 'people-revoke',
	templateUrl: './revoke-page.component.html',
	styleUrls: ['./revoke-page.component.scss']
})
export class RevokePageComponent extends RolePage {
	revokeRoles(roles: Array<string>): Promise<object> {
		const peopleIds = this.workspaceService
			.getSelectedPeople()
			.getValue()
			.map(({ Id }) => Id);
		return this.rolesService.revokeRoles(peopleIds, roles);
	}

	save() {
		this.revokeRoles(this.selectedRoles).then(ok => {
			this.workspaceService.update();
			const people = this.workspaceService
				.getSelectedPeople()
				.getValue()
				.map(person => {
					const roles = person.Roles.filter(r => !this.selectedRoles.includes(r.Id));
					return {
						...person,
						Roles: roles
					};
				});
			this.searchOverridesService.mergeOptimistic(people, ['Roles']);
			super.save();
		});
	}
}
