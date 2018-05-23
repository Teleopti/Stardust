import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs';
import { SearchOverridesService, PeopleOverride } from './search-overrides.service';

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

	searchPeople(query: PeopleSearchQuery): Observable<PeopleSearchResult> {
		return this.http.post('../api/Search/FindPeople', query) as Observable<PeopleSearchResult>;
	}

	async getPersons({ Date = '2017-02-08', PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<
			Array<Person>
		>;
	}
}
