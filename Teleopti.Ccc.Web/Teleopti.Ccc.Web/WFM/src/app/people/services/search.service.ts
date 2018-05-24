import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { SearchOverridesService, PeopleOverride } from './search-overrides.service';
import { tap } from 'rxjs/operators';

export interface PeopleSearchResult {
	People: Array<Person>;
	TotalRows: number;
}

export interface PeopleSearchQuery {
	keyword: string;
	pageIndex: number;
	pageSize: number;
}

@Injectable()
export class SearchService {
	constructor(private http: HttpClient) {}

	public peopleCache$ = new BehaviorSubject<Array<Person>>([]);
	public keyword: string = '';
	public pageIndex: number = 0;
	public pageSize: number = 20;
	public lastQuerySize: number = 0;

	searchPeople(query: PeopleSearchQuery): Observable<PeopleSearchResult> {
		return this.http.post('../api/Search/FindPeople', query).pipe(
			tap((result: PeopleSearchResult) => {
				this.peopleCache$.next(result.People);
				this.lastQuerySize = result.TotalRows;
			})
		) as Observable<PeopleSearchResult>;
	}

	async getPersons({ Date = '2017-02-08', PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<
			Array<Person>
		>;
	}
}
