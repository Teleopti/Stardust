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
	private peopleCache$ = new BehaviorSubject<Array<Person>>([]);

	// people observable
	// 	peopleCache:Person[]
	// 	overrides:Person[]

	public keyword: string = '';
	public pageIndex: number = 0;
	public pageSize: number = 20;

	constructor(private http: HttpClient, private overridesService: SearchOverridesService) {
		overridesService.getOverrides().subscribe({
			next: (overrides: PeopleOverride) => {
				this.peopleCache$.next(overridesService.applyOverrides(this.peopleCache$));
			}
		});
	}

	public getPerson(id: string): Person {
		return this.peopleCache$.getValue().find(p => p.Id === id);
	}

	public getPeople() {
		return this.peopleCache$;
	}

	async searchPeople(query: PeopleSearchQuery): Promise<PeopleSearchResult> {
		['keyword', 'pageIndex', 'pageSize'].forEach(key => {
			this[key] = query[key];
		});
		const res = (await this.http.post('../api/Search/FindPeople', query).toPromise()) as PeopleSearchResult;
		this.peopleCache$.next(res.People);
		return res;
	}

	async getPersons({ Date = '2017-02-08', PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<
			Array<Person>
		>;
	}
}
