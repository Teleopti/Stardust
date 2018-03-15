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

	grantRoles(roles: Array<string>): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.grantRoles(peopleIds, roles).then(ok => {
			this.workspaceService.update();
		});
	}

	save() {
		this.grantRoles(this.selectedRoles);
		super.save();
	}
}
