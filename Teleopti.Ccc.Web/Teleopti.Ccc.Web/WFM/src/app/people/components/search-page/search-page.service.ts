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

		return this.searchService.searchPeople(query);
	}

	searchPeopleAllPages(): Observable<PeopleSearchResult> {
		const query: PeopleSearchQuery = {
			keyword: this.searchService.keyword,
			pageIndex: 0,
			pageSize: this.searchService.lastQuerySize
		};
		return this.searchService.searchPeople(query);
	}
}
