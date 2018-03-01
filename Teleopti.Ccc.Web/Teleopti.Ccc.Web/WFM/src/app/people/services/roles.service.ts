import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class RolesService {
	constructor(private http: HttpClient) { }

	async getRoles(): Promise<Array<Role>> {
		return this.http.get('../api/PeopleData/fetchRoles').toPromise() as Promise<Array<Role>>
	}

	async grantRoles(persons: Array<String>, roles: Array<String>): Promise<object> {
		return await this.http.post('../api/PeopleCommand/grantRoles', { Persons: persons, Roles: roles }).toPromise()
	}

	async revokeRoles(persons: Array<String>, roles: Array<String>): Promise<object> {
		return await this.http.post('../api/PeopleCommand/revokeRoles', { Persons: persons, Roles: roles }).toPromise()
	}
}
