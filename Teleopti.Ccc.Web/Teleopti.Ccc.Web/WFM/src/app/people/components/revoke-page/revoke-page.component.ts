import { Component, Inject, OnInit } from '@angular/core';
import { Person, Role } from '../../types';
import { RolePage } from '../shared/role-page';

@Component({
	selector: 'people-revoke',
	templateUrl: './revoke-page.component.html',
	styleUrls: ['./revoke-page.component.scss']
})
export class RevokePageComponent extends RolePage {
	revokeRoles(roles: Array<string>): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.revokeRoles(peopleIds, roles).then(ok => {
			this.workspaceService.update();
		});
	}

	save() {
		this.revokeRoles(this.selectedRoles);
		super.save();
	}
}
