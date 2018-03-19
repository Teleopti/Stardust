import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient, HttpParams } from '@angular/common/http';

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
	protected peopleCache: Array<Person> = [];

	public keyword: string = '';
	public pageIndex: number = 0;
	public pageSize: number = 20;

	constructor(private http: HttpClient) {}

	public getPerson(id: string): Person {
		return this.peopleCache.find(p => p.Id === id);
	}

	public getPeople(): Array<Person> {
		return this.peopleCache;
	}

	async searchPeople(query: PeopleSearchQuery): Promise<PeopleSearchResult> {
		['keyword', 'pageIndex', 'pageSize'].forEach(key => {
			this[key] = query[key];
		});
		const res = (await this.http.post('../api/Search/FindPeople', query).toPromise()) as PeopleSearchResult;
		this.peopleCache = res.People;
		return res;
	}

	async getPersons({ Date = '2017-02-08', PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<
			Array<Person>
		>;
	}
}
