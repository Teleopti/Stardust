import { Component, OnInit } from '@angular/core';
import { MatTableDataSource, PageEvent } from '@angular/material';

import { NavigationService, PeopleSearchQuery, RolesService, SearchService, WorkspaceService } from '../../services';
import { Person, Role } from '../../types';
import { TogglesService } from '../../../core/services';

@Component({
	selector: 'people-search-page',
	templateUrl: './search-page.component.html',
	styleUrls: ['./search-page.component.scss']
})
export class SearchPageComponent implements OnInit {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public rolesService: RolesService,
		public searchService: SearchService // private toggles: TogglesService
	) {
		// toggles.getToggles().subscribe({
		// 	next: toggles => {
		// 		console.log(toggles);
		// 	}
		// });
	}

	roles: Array<Role> = [];

	displayedColumns = ['select', 'FirstName', 'LastName', 'Site', 'Team', 'Roles'];
	dataSource = new MatTableDataSource<Person>([]);
	searchQuery = '';

	pagination = {
		length: 0, // Get from API
		pageSizeOptions: [20, 50, 100, 500]
	};

	ngOnInit() {
		this.searchQuery = this.searchService.keyword;
		this.searchService.getPeople().subscribe({
			next: (people: Person[]) => {
				this.dataSource.data = people;
			}
		});
	}

	onSearch() {
		this.searchService.keyword = this.searchQuery;
		this.searchPeople();
	}

	searchPeople() {
		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageSize: this.searchService.pageSize,
			pageIndex: this.searchService.pageIndex
		};
		return this.searchService.searchPeople(query).then(searchResult => {
			this.pagination.length = searchResult.TotalRows;
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
	}

	personToRoles(person: Person): string {
		return person.Roles.map((role: Role) => role.Name)
			.sort((r1, r2) => r1.localeCompare(r2))
			.join(', ');
	}

	paginationChanged(event: PageEvent) {
		this.searchService.pageSize = event.pageSize;
		this.searchService.pageIndex = event.pageIndex;
		this.searchPeople();
	}

	toggleRow(row: Person) {
		this.toggleSelectedPerson(row.Id);
	}
}
