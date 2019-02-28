import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { Person, Role } from '../../../shared/types';
import {
	COLUMNS,
	DIRECTION,
	NavigationService,
	PeopleSearchQuery,
	PeopleSearchResult,
	RolesService,
	SearchService,
	WorkspaceService
} from '../../shared';
import { SearchPageService } from './search-page.service';
import { TogglesService } from '../../../core/services';

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
		public searchService: SearchService,
		public togglesService: TogglesService
	) {
		this.togglesService.toggles$.subscribe({
			next: toggles => {
				this.improvedSearchActive = toggles.Wfm_PeopleWeb_Improve_Search_81681;
			}
		});
	}

	improvedSearchActive = false;
	displayedColumns = ['select', 'FirstName', 'LastName', 'SiteTeam', 'Roles'];
	searchControl = new FormControl('');
	peopleDataSet: Person[];
	isSearching = false;

	pagination = {
		length: 0, // Get from API
		pageSizeOptions: [20, 50, 100, 500]
	};

	ngOnInit() {
		this.searchControl.setValue(this.searchService.keyword);
		this.pagination.length = this.searchService.lastQuerySize;
		this.searchPageService.getPeople().subscribe({
			next: (people: Person[]) => {
				this.peopleDataSet = people;
			}
		});
		console.log(this.improvedSearchActive);
		if (this.improvedSearchActive === false) {
			this.searchControl.valueChanges.pipe(debounceTime(700)).subscribe({ next: () => this.onSearch() });
		}
	}

	nzOnCurrentPageDataChange($event) {}

	nzOnPageIndexChange($event: number) {
		this.searchService.pageIndex = $event - 1;

		this.searchPeople();
	}

	nzOnPageSizeChange($event: number) {
		this.searchService.pageSize = $event;
		this.searchPeople();
	}

	sort(column: string) {
		if (typeof COLUMNS[column] === 'undefined') console.warn('Sort column not defined');
		const columnId: COLUMNS = COLUMNS[column];
		const columnUnchanged = columnId === this.searchService.sortColumn;
		let direction: DIRECTION;
		if (columnUnchanged) {
			if (this.searchService.direction === DIRECTION.asc) direction = DIRECTION.desc;
			if (this.searchService.direction === DIRECTION.desc) direction = DIRECTION.asc;
		} else {
			direction = DIRECTION.asc;
		}

		this.searchService.sortColumn = COLUMNS[column];
		this.searchService.direction = direction;
		this.searchPeople();
	}

	isSortingBy(column: string) {
		return COLUMNS[column] === this.searchService.sortColumn;
	}

	isSortingAsc() {
		return DIRECTION.asc === this.searchService.direction;
	}

	isSortingDesc() {
		return DIRECTION.desc === this.searchService.direction;
	}

	isAllSelected() {
		if (this.peopleDataSet.length === 0) return false;
		const numSelectedOnPage = this.peopleDataSet.filter(person => this.workspaceService.isPersonSelected(person.Id))
			.length;
		const numRows = this.peopleDataSet.length;
		return numSelectedOnPage === numRows;
	}

	isAnySelected() {
		const numSelectedOnPage = this.peopleDataSet.filter(person => this.workspaceService.isPersonSelected(person.Id))
			.length;
		return numSelectedOnPage > 0;
	}

	onSearch() {
		this.searchService.keyword = this.searchControl.value;
		this.searchService.pageIndex = 0;
		this.searchPeople();
	}

	searchPeople() {
		if (this.improvedSearchActive === true) {
			if (this.isSearching) {
				return;
			}
			this.isSearching = true;
		}

		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageSize: this.searchService.pageSize,
			pageIndex: this.searchService.pageIndex,
			sortColumn: this.searchService.sortColumn,
			direction: this.searchService.direction
		};
		this.searchPageService.searchPeople(query).subscribe({
			next: searchResult => {
				if (this.improvedSearchActive === true) {
					this.isSearching = false;
				}
				this.pagination.length = searchResult.TotalRows;
			}
		});
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

	selectAllOnPage() {
		if (this.isAllSelected()) {
			this.peopleDataSet.forEach(person => {
				if (this.workspaceService.isPersonSelected(person.Id)) this.workspaceService.deselectPerson(person);
			});
		} else {
			this.peopleDataSet.forEach(person => {
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
