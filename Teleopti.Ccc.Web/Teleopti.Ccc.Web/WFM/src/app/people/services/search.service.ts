import { Injectable } from '@angular/core';
import { Person } from '../types';
import { HttpClient } from '@angular/common/http';


@Injectable()
export class SearchService {
	protected peopleCache: Array<Person> = [];

	constructor(private http: HttpClient) { }

	public getPerson(id: string): Person {
		return this.peopleCache.find(p => p.Id === id);
	}

	public getPeople(): Array<Person> {
		return this.peopleCache;
	}

	async searchPeople(keyword = 'a'): Promise<Array<Person>> {
		const sortPeopleByName = (a: Person, b: Person) => {
			if (a.FirstName >= b.FirstName) return 1;
			if (a.FirstName < b.FirstName) return -1;
			return 0;
		};

		interface SearchPerson {
			PersonId: string
		}
		interface SearchResult {
			People: Array<SearchPerson>,
			CurrentPage: number,
			OptionalColumns: Array<string>,
			TotalPages: number
		}
		const searchPeople = async (): Promise<Array<SearchPerson>> => {
			const result = this.http.get(`../api/Search/People/Keyword?currentPageIndex=1&keyword=${keyword}&pageSize=10&sortColumns=LastName:true`).toPromise() as Promise<SearchResult>
			return result.then(({ People }) => People)
		};

		const getPersonIds = async () => searchPeople().then(people => people.map(person => person.PersonId))

		const getPersons = async ({ Date = '2017-02-08', PersonIdList }) => {
			return this.http.post('../api/PeopleData/fetchPersons', { Date, PersonIdList }).toPromise() as Promise<Array<Person>>
		};
		const PersonIdList = await getPersonIds()

		let people = await getPersons({ PersonIdList });

		people.sort(sortPeopleByName);

		this.peopleCache = people;
		return people;
	}
}
