import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import 'rxjs/add/operator/takeUntil';
import { NavigationService, WorkspaceService } from '../../services';
import { Person, Role } from '../../types';
import { GrantPageService } from './grant-page.service';
@Component({
	selector: 'people-grant',
	templateUrl: './grant-page.component.html',
	styleUrls: ['./grant-page.component.scss'],
	providers: [GrantPageService]
})
export class GrantPageComponent implements OnInit, OnDestroy {
	constructor(
		public nav: NavigationService,
		public grantPageService: GrantPageService,
		public workspaceService: WorkspaceService
	) {}

	private componentDestroyed: Subject<any> = new Subject();

	roles: Role[] = [];
	people: Person[] = [];
	selectedRoles: Role[] = [];

	ngOnInit() {
		this.grantPageService.roles$.subscribe({
			next: (roles: Role[]) => {
				this.roles = roles;
			}
		});
		this.workspaceService.people$.takeUntil(this.componentDestroyed).subscribe({
			next: (people: Person[]) => {
				if (people.length === 0) return this.nav.navToSearch();
				this.people = people;
			}
		});
	}

	ngOnDestroy() {
		this.componentDestroyed.next();
		this.componentDestroyed.complete();
	}

	isRoleOnAll(roleId: string): boolean {
		const roleOccurancesInPeople = this.people.filter(({ Roles }) => Roles.map(role => role.Id).includes(roleId))
			.length;
		const numberOfPeople = this.workspaceService.getSelectedPeople().getValue().length;
		return roleOccurancesInPeople === numberOfPeople;
	}

	getRolesNotOnAll(): Role[] {
		return this.roles.filter(role => !this.isRoleOnAll(role.Id));
	}

	toggleSelectedRole(role: Role): void {
		this.selectedRoles = this.selectedRoles.find(r => r.Id === role.Id)
			? this.selectedRoles.filter(r => r.Id !== role.Id)
			: this.selectedRoles.concat(role);
	}

	isRoleSelected(roleId: string): boolean {
		return !!this.selectedRoles.find(r => r.Id === roleId);
	}

	isAnyRoleSelected(): boolean {
		return this.selectedRoles.length > 0;
	}

	save() {
		this.grantPageService.grantRoles(this.selectedRoles, this.people).subscribe({
			next: () => this.close()
		});
	}

	close() {
		this.nav.navToSearch();
	}
}
