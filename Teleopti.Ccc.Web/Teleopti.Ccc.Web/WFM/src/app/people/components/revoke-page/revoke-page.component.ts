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
			super.save();
		});
	}
}
