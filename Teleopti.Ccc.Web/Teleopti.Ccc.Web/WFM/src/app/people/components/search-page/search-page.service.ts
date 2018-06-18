import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
	COLUMNS,
	DIRECTION,
	PeopleOverride,
	PeopleSearchQuery,
	PeopleSearchResult,
	SearchOverridesService,
	SearchService,
	WorkspaceService
} from '../../services';
import { Person } from '../../types';

@Injectable()
export class SearchPageService {
	constructor(
		private overridesService: SearchOverridesService,
		public workspaceService: WorkspaceService,
		public searchService: SearchService
	) {
		overridesService.getOverrides().subscribe({
			next: (overrides: PeopleOverride) => {
				this.peopleCache$.next(overridesService.applyOverrides(this.peopleCache$));
			}
		});
	}

	private peopleCache$ = this.searchService.peopleCache$;

	public getPerson(id: string): Person {
		return this.peopleCache$.getValue().find(p => p.Id === id);
	}

	public getPeople() {
		return this.peopleCache$;
	}

	searchPeople(query: PeopleSearchQuery): Observable<PeopleSearchResult> {
		['keyword', 'pageIndex', 'pageSize'].forEach(key => {
			this[key] = query[key];
		});

		return this.searchService.searchPeople(query).pipe(
			tap((result: PeopleSearchResult) => {
				this.searchService.lastQuerySize = result.TotalRows;
				this.peopleCache$.next(result.People);
			})
		);
	}

	searchPeopleAllPages(): Observable<PeopleSearchResult> {
		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageIndex: 0,
			pageSize: this.searchService.lastQuerySize,
			sortColumn: COLUMNS.LastName,
			direction: DIRECTION.asc
		};
		return this.searchService.searchPeople(query);
	}
}
