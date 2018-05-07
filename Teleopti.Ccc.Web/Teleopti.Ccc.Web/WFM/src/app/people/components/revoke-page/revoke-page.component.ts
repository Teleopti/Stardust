import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { NavigationService, WorkspaceService } from '../../services';
import { Person, Role } from '../../types';
import { RevokePageService } from './revoke-page.service';

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
	selectedRoles: Role[] = [];

	private componentDestroyed: Subject<any> = new Subject();

	ngOnInit() {
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
		this.revokePageService.revokeRoles(this.selectedRoles, this.people).subscribe({
			next: () => this.close()
		});
	}

	close() {
		this.nav.navToSearch();
	}
}
