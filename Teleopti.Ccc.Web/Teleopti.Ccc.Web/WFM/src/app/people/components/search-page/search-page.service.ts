import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { of } from 'rxjs/observable/of';
import { flatMap, map, tap } from 'rxjs/operators';
import {
	SearchService,
	WorkspaceService,
	PeopleSearchResult,
	PeopleSearchQuery,
	SearchOverridesService,
	PeopleOverride
} from '../../services';
import { Person } from '../../types';
import { PeopleSearch } from '../../../../../e2e/people/search.po';

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

	private peopleCache$ = new BehaviorSubject<Array<Person>>([]);

	public keyword: string = '';
	public pageIndex: number = 0;
	public pageSize: number = 20;
	public lastQuerySize: number = 0;

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
				this.peopleCache$.next(result.People);
				this.lastQuerySize = result.TotalRows;
			})
		);
	}

	searchPeopleAllPages(): Observable<PeopleSearchResult> {
		const query: PeopleSearchQuery = {
			keyword: this.keyword,
			pageIndex: 0,
			pageSize: this.lastQuerySize
		};
		return this.searchService.searchPeople(query);
	}
}
