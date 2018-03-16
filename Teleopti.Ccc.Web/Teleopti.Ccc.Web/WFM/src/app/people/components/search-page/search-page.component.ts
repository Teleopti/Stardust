import { Component, OnInit, ViewChild } from '@angular/core';
import { WorkspaceService, RolesService, SearchService, NavigationService, PeopleSearchQuery } from '../../services';
import { Role, Person } from '../../types';
import { MatSort, PageEvent, MatTableDataSource } from '@angular/material';

@Component({
	selector: 'people-search-page',
	templateUrl: './search-page.component.html',
	styleUrls: ['./search-page.component.scss']
})
export class SearchPageComponent implements OnInit {
	constructor(
		protected nav: NavigationService,
		protected workspaceService: WorkspaceService,
		public rolesService: RolesService,
		public searchService: SearchService
	) {}

	@ViewChild(MatSort) sort: MatSort;

	roles: Array<Role> = [];

	displayedColumns = ['select', 'FirstName', 'LastName', 'Roles', 'Site', 'Team'];
	dataSource = new MatTableDataSource<Person>(this.searchService.getPeople());

	pagination = {
		length: 0, // Get from API
		pageSizeOptions: [20, 50, 100, 500]
	};

	ngOnInit() {
		this.searchPeople();
	}

	ngAfterViewInit() {
		this.dataSource.sort = this.sort;
	}

	searchPeople() {
		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageSize: this.searchService.pageSize,
			pageIndex: this.searchService.pageIndex
		};
		return this.searchService.searchPeople(query).then(searchResult => {
			this.pagination.length = searchResult.TotalRows;
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
