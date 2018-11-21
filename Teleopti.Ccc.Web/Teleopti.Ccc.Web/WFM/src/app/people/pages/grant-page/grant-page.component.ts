import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Person, Role } from '../../../shared/types';
import { NavigationService, WorkspaceService } from '../../shared';
import { GrantPageService } from './grant-page.service';

interface SelectableRole extends Role {
	selected: boolean;
}

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
	selectableRoles: SelectableRole[] = [];

	ngOnInit() {
		this.grantPageService.roles$.subscribe({
			next: (roles: Role[]) => {
				this.roles = roles;
				const selectableRoles = roles.map(role => ({ ...role, selected: false }));
				this.selectableRoles = selectableRoles;
			}
		});
		this.workspaceService.people$.pipe(takeUntil(this.componentDestroyed)).subscribe({
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
		const numberOfPeople = this.people.length;
		return roleOccurancesInPeople === numberOfPeople;
	}

	getRolesNotOnAll(): SelectableRole[] {
		return this.selectableRoles.filter(role => !this.isRoleOnAll(role.Id));
	}

	toggleSelectedRole(role: Role): void {
		const roleId = role.Id;
		this.selectableRoles = this.selectableRoles.map(r => {
			if (roleId === role.Id)
				return {
					...r,
					selected: !r.selected
				};
			else return r;
		});
	}

	isRoleSelected(roleId: string): boolean {
		return !!this.selectableRoles.find(r => r.Id === roleId && r.selected);
	}

	isAnyRoleSelected(): boolean {
		return this.selectableRoles.filter(role => role.selected).length > 0;
	}

	save() {
		const selectedRoles = this.selectableRoles.filter(role => role.selected);
		this.grantPageService.grantRoles(selectedRoles, this.people).subscribe({
			next: () => this.close()
		});
	}

	close() {
		this.nav.navToSearch();
	}
}
