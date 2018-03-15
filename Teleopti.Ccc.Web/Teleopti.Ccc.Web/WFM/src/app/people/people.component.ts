import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { PageEvent, MatTableDataSource, MatSort } from '@angular/material';
import { Person, Role } from './types';

import { Observable } from 'rxjs/Rx';
import { WorkspaceService, RolesService, SearchService } from './services';
import { GrantPageComponent } from './components';
import { SelectionModel } from '@angular/cdk/collections';

@Component({
	selector: 'app-people',
	templateUrl: './people.component.html',
	styleUrls: ['./people.component.scss']
})
export class PeopleComponent implements OnInit {
	constructor(
		protected workspaceService: WorkspaceService,
		public rolesService: RolesService,
		public searchService: SearchService
	) {}

	roles: Array<Role> = [];

	pagination = {
		length: 0, // Get from API
		pageSize: 10,
		pageIndex: 0,
		pageSizeOptions: [20, 50, 100, 500]
	};

	ngOnInit() {
		this.searchPeople();
		this.rolesService.getRoles().then(roles => {
			this.roles = roles;
		});
	}

	@ViewChild(MatSort) sort: MatSort;

	ngAfterViewInit() {
		this.dataSource.sort = this.sort;
	}

	searchPeople() {
		return this.searchService
			.searchPeople({
				pageSize: this.pagination.pageSize,
				pageIndex: this.pagination.pageIndex
			})
			.then(searchResult => {
				this.pagination.length = searchResult.people.length * searchResult.pages;
				this.dataSource.data = this.searchService.getPeople();
				return searchResult;
			});
	}

	toggleSelectedPerson(id: string): void {
		const isSelected = this.workspaceService.isPersonSelected(id);
		if (isSelected) {
			const person = this.workspaceService.getSelectedPerson(id);
			this.workspaceService.deselectPerson(person);
		} else {
			const person = this.searchService.getPerson(id);
			this.workspaceService.selectPerson(person);
		}

		// Workspace
		if (this.workspaceService.getSelectedPeople().length === 0) {
			this.displayGrantView = false;
			this.displayRevokeView = false;
		}
	}

	personToRoles(person: Person): string {
		return person.Roles.map((role: Role) => role.Name)
			.sort((r1, r2) => r1.localeCompare(r2))
			.join(', ');
	}

	grantRoles(roles): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.grantRoles(peopleIds, roles).then(ok => {
			this.searchPeople();
			this.workspaceService.update();
		});
	}

	revokeRoles(roles): void {
		const peopleIds = this.workspaceService.getSelectedPeople().map(({ Id }) => Id);
		this.rolesService.revokeRoles(peopleIds, roles).then(ok => {
			this.searchPeople();
			this.workspaceService.update();
		});
	}

	/** Below is window grant component */
	private _displayGrantView = false;
	private _displayRevokeView = false;

	get displayGrantView(): boolean {
		if (!this.workspaceService.isAnySelected()) this._displayGrantView = false;
		return this._displayGrantView;
	}
	get displayRevokeView(): boolean {
		if (!this.workspaceService.isAnySelected()) this._displayRevokeView = false;
		return this._displayRevokeView;
	}

	set displayGrantView(shouldShow) {
		this._displayGrantView = shouldShow;
	}

	set displayRevokeView(shouldShow) {
		this._displayRevokeView = shouldShow;
	}

	toggleGrantView(): void {
		this.displayGrantView = !this.displayGrantView;
	}
	toggleRevokeView(): void {
		this.displayRevokeView = !this.displayRevokeView;
	}

	handleGranted(roles: Array<Role>): void {
		if (roles.length > 0) this.grantRoles(roles);
		this.toggleGrantView();
	}

	handleRevoked(roles: Array<Role>): void {
		if (roles.length > 0) this.revokeRoles(roles);
		this.toggleRevokeView();
	}

	//#region Pagination
	paginationChanged(event: PageEvent) {
		this.pagination.pageSize = event.pageSize;
		this.pagination.pageIndex = event.pageIndex;
		this.searchPeople();
	}

	//#endregion

	//region tabletest
	displayedColumns = ['select', 'FirstName', 'LastName', 'Roles', 'Site', 'Team'];
	dataSource = new MatTableDataSource<Person>(this.searchService.getPeople());

	toggleRow(row: Person) {
		this.toggleSelectedPerson(row.Id);
	}

	//endregion
}
