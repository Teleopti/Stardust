import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class RolesService {
	constructor(private http: HttpClient) {}

	protected rolesCache: Array<Role> = [];

	async getRoles(): Promise<Array<Role>> {
		const roles = (await this.http.get('../api/PeopleData/fetchRoles').toPromise()) as Array<Role>;
		this.rolesCache = roles.sort((r1, r2) => r1.Name.localeCompare(r2.Name));
		return this.rolesCache;
	}

	async grantRoles(persons: Array<String>, roles: Array<String>): Promise<object> {
		return await this.http.post('../api/PeopleCommand/grantRoles', { Persons: persons, Roles: roles }).toPromise();
	}

	async revokeRoles(persons: Array<String>, roles: Array<String>): Promise<object> {
		return await this.http.post('../api/PeopleCommand/revokeRoles', { Persons: persons, Roles: roles }).toPromise();
	}
}
