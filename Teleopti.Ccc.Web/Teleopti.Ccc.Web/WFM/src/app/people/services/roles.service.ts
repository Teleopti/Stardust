import { Injectable } from '@angular/core';
import { Person, Role } from '../types';

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

// Fetch wrapper for json
const fetchJSON = async (input, init) => {
	return fetch(input, init).then(response => response.json());
};

// Array.map
const personToId = ({ PersonId }) => PersonId;

// Array.sort
const sortRolesByName = (role1, role2) => role1.Name >= role2.Name;
const sortPeopleByName = (person1, person2) => person1.FirstName >= person2.FirstName;

// API calls
const searchPeople = async (keyword = 'a') => {
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

@Injectable()
export class RolesService {
	getPeople = async (): Promise<Array<Person>> => {
		const peopleResult = await searchPeople();
		const PersonIdList = peopleResult.map(personToId);

		const persons = await getPersons({ PersonIdList });
		return persons
			.map(person => ({
				...person
			}))
			.sort(sortPeopleByName);
	};

	getRoles = async (): Promise<Array<Role>> => {
		return fetchJSON('../api/PeopleData/fetchRoles', fetchDefaultOptions);
	};

	grantRoles = async (persons: Array<String>, roles: Array<String>): Promise<Boolean> => {
		const requestInit: RequestInit = {
			...fetchDefaultOptions,
			method: 'POST',
			body: JSON.stringify({ Persons: persons, Roles: roles })
		};
		const response = await fetch(`../api/PeopleCommand/grantRoles`, requestInit);

		return response.ok;
	};

	revokeRoles = async (persons: Array<String>, roles: Array<String>): Promise<Boolean> => {
		const requestInit: RequestInit = {
			...fetchDefaultOptions,
			method: 'POST',
			body: JSON.stringify({ Persons: persons, Roles: roles })
		};
		const response = await fetch(`../api/PeopleCommand/revokeRoles`, requestInit);

		return response.ok;
	};

	constructor() {}
}
