import { Component, OnInit } from '@angular/core';
import { MatTableDataSource, PageEvent, Sort } from '@angular/material';

import {
	NavigationService,
	PeopleSearchQuery,
	RolesService,
	WorkspaceService,
	PeopleSearchResult,
	SearchService,
	COLUMNS,
	DIRECTION
} from '../../services';
import { Person, Role } from '../../types';
import { FormControl } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { SearchPageService } from './search-page.service';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { SrvRecord } from 'dns';

interface SortModel {
	active: 'FirstName' | 'LastName' | 'EmpNum' | 'Note' | 'TerminalDate' | 'TeamSite';
	direction: 'asc' | 'desc' | '';
}

@Component({
	selector: 'people-search-page',
	templateUrl: './search-page.component.html',
	styleUrls: ['./search-page.component.scss'],
	providers: [SearchPageService]
})
export class SearchPageComponent implements OnInit {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public rolesService: RolesService,
		public searchPageService: SearchPageService,
		public searchService: SearchService
	) {}

	displayedColumns = ['select', 'FirstName', 'LastName', 'SiteTeam', 'Roles'];
	dataSource = new MatTableDataSource<Person>([]);
	searchControl = new FormControl('');

	pagination = {
		length: 0, // Get from API
		pageSizeOptions: [20, 50, 100, 500]
	};

	ngOnInit() {
		this.searchControl.setValue(this.searchService.keyword);
		this.pagination.length = this.searchService.lastQuerySize;
		this.searchPageService.getPeople().subscribe({
			next: (people: Person[]) => {
				this.dataSource.data = people;
			}
		});
		this.searchControl.valueChanges.pipe(debounceTime(700)).subscribe({ next: () => this.onSearch() });
	}

	isAllSelected() {
		if (this.dataSource.data.length === 0) return false;
		const numSelectedOnPage = this.dataSource.data.filter(person =>
			this.workspaceService.isPersonSelected(person.Id)
		).length;
		const numRows = this.dataSource.data.length;
		return numSelectedOnPage === numRows;
	}

	isAnySelected() {
		const numSelectedOnPage = this.dataSource.data.filter(person =>
			this.workspaceService.isPersonSelected(person.Id)
		).length;
		return numSelectedOnPage > 0;
	}

	onSearch() {
		this.searchService.keyword = this.searchControl.value;
		this.searchPeople();
	}

	searchPeople() {
		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageSize: this.searchService.pageSize,
			pageIndex: this.searchService.pageIndex,
			sortColumn: this.searchService.sortColumn,
			direction: this.searchService.direction
		};
		this.searchPageService.searchPeople(query).subscribe({
			next: searchResult => {
				this.pagination.length = searchResult.TotalRows;
			}
		});
	}

	sortData(data: SortModel) {
		if (data.direction === '') {
			this.searchService.sortColumn = COLUMNS.LastName;
			this.searchService.direction = DIRECTION.asc;
		} else {
			this.searchService.sortColumn = COLUMNS[data.active];
			this.searchService.direction = DIRECTION[data.direction];
		}

		this.searchPeople();
	}

	toggleSelectedPerson(id: string): void {
		const isSelected = this.workspaceService.isPersonSelected(id);
		if (isSelected) {
			const person = this.workspaceService.getSelectedPerson(id);
			this.workspaceService.deselectPerson(person);
		} else {
			const person = this.searchPageService.getPerson(id);
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

	selectAllOnPage() {
		if (this.isAllSelected()) {
			this.dataSource.data.forEach(person => {
				if (this.workspaceService.isPersonSelected(person.Id)) this.workspaceService.deselectPerson(person);
			});
		} else {
			this.dataSource.data.forEach(person => {
				if (!this.workspaceService.isPersonSelected(person.Id)) this.workspaceService.selectPerson(person);
			});
		}
	}

	selectAllMatches() {
		this.searchPageService.searchPeopleAllPages().subscribe({
			next: (result: PeopleSearchResult) => {
				this.workspaceService.selectPeople(result.People);
			}
		});
	}
}
