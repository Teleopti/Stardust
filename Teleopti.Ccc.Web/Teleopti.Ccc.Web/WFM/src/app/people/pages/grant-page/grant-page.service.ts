import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Person, Role } from '../../../shared/types';
import { RolesService, SearchOverridesService, WorkspaceService } from '../../shared';
@Injectable()
export class GrantPageService {
	constructor(
		public rolesService: RolesService,
		public workspaceService: WorkspaceService,
		private searchOverridesService: SearchOverridesService
	) {}

	public roles$: Observable<Role[]> = this.rolesService.getRoles();
	public rolesOfPeople$: Observable<Role[]> = this.workspaceService.people$.pipe(
		map(people => this.peopleToRoles(people))
	);

	private peopleToRoles(people: Person[]) {
		const uniqueRoles = [];
		people.forEach(({ Roles }) => {
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

	grantRoles(roles: Role[], people: Person[]) {
		const roleIds = roles.map(role => role.Id);
		const peopleIds = people.map(({ Id }) => Id);
		return this.rolesService.grantRoles(peopleIds, roleIds).pipe(
			tap(() => {
				this.workspaceService.update();

				const peopleForOverrides = people.map(person => {
					const personRoles = person.Roles.filter(r => !roleIds.includes(r.Id)).concat(roles);
					return {
						...person,
						Roles: personRoles
					};
				});

				this.searchOverridesService.mergeOptimistic(peopleForOverrides, ['Roles']);
			})
		);
	}
}
