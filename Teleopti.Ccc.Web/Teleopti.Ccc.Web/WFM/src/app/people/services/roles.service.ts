import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient } from '@angular/common/http';

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

// Array.sort
const sortRolesByName = (role1, role2) => role1.Name >= role2.Name;

@Injectable()
export class RolesService {
	// constructor(private http: HttpClient) {

	// }

	// async getRoles(): Promise<Array<Role>> {
	// 	this.http.get('../api/PeopleData/fetchRoles').filter
	// }
	async getRoles(): Promise<Array<Role>> {
		return fetchJSON('../api/PeopleData/fetchRoles', fetchDefaultOptions);
	}

	async grantRoles(persons: Array<String>, roles: Array<String>): Promise<Boolean> {
		const requestInit: RequestInit = {
			...fetchDefaultOptions,
			method: 'POST',
			body: JSON.stringify({ Persons: persons, Roles: roles })
		};
		const response = await fetch(`../api/PeopleCommand/grantRoles`, requestInit);

		return response.ok;
	}

	async revokeRoles(persons: Array<String>, roles: Array<String>): Promise<Boolean> {
		const requestInit: RequestInit = {
			...fetchDefaultOptions,
			method: 'POST',
			body: JSON.stringify({ Persons: persons, Roles: roles })
		};
		const response = await fetch(`../api/PeopleCommand/revokeRoles`, requestInit);

		return response.ok;
	}

}
