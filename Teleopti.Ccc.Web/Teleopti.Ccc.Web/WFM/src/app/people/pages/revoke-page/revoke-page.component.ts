import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Person, Role } from '../../../shared/types';
import { NavigationService, WorkspaceService } from '../../shared';
import { RevokePageService } from './revoke-page.service';

interface SelectableRole extends Role {
	selected: boolean;
}
@Component({
	selector: 'people-revoke',
	templateUrl: './revoke-page.component.html',
	styleUrls: ['./revoke-page.component.scss'],
	providers: [RevokePageService]
})
export class RevokePageComponent implements OnInit, OnDestroy {
	constructor(
		public nav: NavigationService,
		public revokePageService: RevokePageService,
		public workspaceService: WorkspaceService
	) {}

	people: Person[] = [];
	selectableRoles: SelectableRole[] = [];

	private componentDestroyed: Subject<any> = new Subject();

	ngOnInit() {
		this.revokePageService.rolesOfPeople$.subscribe({
			next: (roles: Role[]) => {
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

	toggleSelectedRole(role: Role): void {
		const roleId = role.Id;
		this.selectableRoles = this.selectableRoles.map(r => {
			if (roleId === r.Id)
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
		this.revokePageService.revokeRoles(selectedRoles, this.people).subscribe({
			next: () => this.close()
		});
	}

	close() {
		this.nav.navToSearch();
	}
}
