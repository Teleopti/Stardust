import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient, HttpParams } from '@angular/common/http';

export interface PeopleSearchResult {
	people: Array<Person>;
	currentPage: number;
	pages: number;
}

export interface PeopleSearchResultNew {
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

	async searchPeople(query: PeopleSearchQuery): Promise<PeopleSearchResultNew> {
		['keyword', 'pageIndex', 'pageSize'].forEach(key => {
			this[key] = query[key];
		});
		const res = (await this.http.post('../api/Search/FindPeople', query).toPromise()) as PeopleSearchResultNew;
		this.peopleCache = res.People;
		return res;
	}

	async searchPeople_old({ keyword = 'a', pageIndex = 0, pageSize = 10 } = {}): Promise<PeopleSearchResult> {
		interface SearchPerson {
			PersonId: string;
		}
		interface SearchResult {
			People: Array<SearchPerson>;
			CurrentPage: number;
			OptionalColumns: Array<string>;
			TotalPages: number;
		}
		const searchPeople = async (): Promise<SearchResult> => {
			const params = new HttpParams()
				.set('currentPageIndex', (++pageIndex).toString()) // uses non-zero index
				.set('keyword', keyword)
				.set('pageSize', pageSize.toString())
				.set('sortColumns', 'LastName:true');
			const result = this.http.get(`../api/Search/People/Keyword`, { params }).toPromise() as Promise<
				SearchResult
			>;

			return result;
		};

		const searchResult: SearchResult = await searchPeople();

		const getPersonIds = (people: Array<SearchPerson>) => people.map(person => person.PersonId);

		const PersonIdList = await getPersonIds(searchResult.People);

		let people = await this.getPersons({ PersonIdList });

		// const sortPeopleByName = (a: Person, b: Person) => {
		// 	if (a.FirstName >= b.FirstName) return 1;
		// 	if (a.FirstName < b.FirstName) return -1;
		// 	return 0;
		// };
		// people.sort(sortPeopleByName);

		this.peopleCache = people;
		return {
			people,
			currentPage: searchResult.CurrentPage,
			pages: searchResult.TotalPages
		};
	}

	async getPersons({ Date = '2017-02-08', PersonIdList }): Promise<Array<Person>> {
		return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<
			Array<Person>
		>;
	}
}
