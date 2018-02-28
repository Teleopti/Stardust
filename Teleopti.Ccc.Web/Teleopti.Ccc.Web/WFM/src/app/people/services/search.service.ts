import { Injectable } from '@angular/core';
import { Person } from '../types';
import { WorkspaceService } from './workspace.service';


@Injectable()
export class SearchService {
	protected peopleCache: Array<Person> = [];

	constructor() { }

	public getPerson(id: string): Person {
		return this.peopleCache.find(p => p.Id === id);
	}

	public getPeople(): Array<Person> {
		return this.peopleCache;
	}

	async searchPeople(keyword = 'a'): Promise<Array<Person>> {
		const personToId = ({ PersonId }) => PersonId;
		const sortPeopleByName = (person1, person2) => person1.FirstName >= person2.FirstName;

		const requestCredentials: RequestCredentials = 'include';

		const fetchDefaultOptions: RequestInit = {
			method: 'GET',
			headers: {
				Accept: 'application/json',
				'Content-Type': 'application/json',
				Cache: 'no-cache'
			},
			credentials: requestCredentials
		};

		const fetchJSON = async (input, init) => {
			return fetch(input, init).then(response => response.json());
		};

		const searchPeople = async () => {
			return fetchJSON(
				`../api/Search/People/Keyword?currentPageIndex=1&keyword=${keyword}&pageSize=10&sortColumns=LastName:true`,
				fetchDefaultOptions
			).then(({ People }) => People);
		};

		const getPersons = async ({ Date = '2017-02-08', PersonIdList }) => {
			return fetchJSON(`../api/PeopleData/fetchPersons`, {
				...fetchDefaultOptions,
				method: 'POST',
				body: JSON.stringify({ Date, PersonIdList })
			});
		};

		const peopleResult = await searchPeople();
		const PersonIdList = peopleResult.map(personToId);

		let people = await getPersons({ PersonIdList });

		people.sort(sortPeopleByName);

		this.peopleCache = people;
		return people;
	}
}
