import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Person } from '../../shared/types';

export interface PeopleSearchResult {
	People: Array<Person>;
	TotalRows: number;
}

export interface PeopleSearchQuery {
	keyword: string;
	pageIndex: number;
	pageSize: number;
	sortColumn: number;
	direction: number;
}

export enum COLUMNS {
	FirstName = 0,
	LastName = 1,
	SiteTeam = 4
}

export enum DIRECTION {
	asc = 1,
	desc = 0
}

@Injectable()
export class SearchService {
	constructor(private http: HttpClient) {}

	public peopleCache$ = new BehaviorSubject<Array<Person>>([]);
	public keyword = '';
	public pageIndex = 0;
	public pageSize = 20;
	public lastQuerySize = 0;
	public sortColumn: number = COLUMNS.LastName;
	public direction: number = DIRECTION.asc;

	searchPeople(query: PeopleSearchQuery): Observable<PeopleSearchResult> {
		return this.http.post('../api/Search/FindPeople', query) as Observable<PeopleSearchResult>;
	}

	async getPersons({ PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { PersonIdList }).toPromise() as Promise<Array<Person>>;
	}
}
