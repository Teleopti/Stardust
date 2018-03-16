import { Component, OnInit } from '@angular/core';
import { Person, Role } from '../../types';

import { RolePage } from '../shared/role-page';

@Component({
	selector: 'people-grant',
	templateUrl: './grant-page.component.html',
	styleUrls: ['./grant-page.component.scss']
})
export class GrantPageComponent extends RolePage implements OnInit {
	ngOnInit() {
		this.rolesService.getRoles().then(roles => {
			this.roles = roles;
		});
	}

	grantRoles(roles: Array<string>): Promise<object> {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		return this.rolesService.grantRoles(peopleIds, roles);
	}

	save() {
		this.grantRoles(this.selectedRoles).then(() => {
			this.workspaceService.update();
			super.save();
		});
	}
}
