import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Role } from '../types';

@Injectable()
export class RolesService {
	constructor(private http: HttpClient) {}

	private sortRoles(roles: Role[]) {
		return roles.sort((r1, r2) => r1.Name.localeCompare(r2.Name));
	}

	getRoles(): Observable<Role[]> {
		return this.http.get('../api/PeopleData/fetchRoles').pipe(map((roles: Role[]) => this.sortRoles(roles)));
	}

	grantRoles(persons: string[], roles: string[]) {
		return this.http.post('../api/PeopleCommand/grantRoles', { Persons: persons, Roles: roles });
	}

	revokeRoles(persons: Array<String>, roles: Array<String>) {
		return this.http.post('../api/PeopleCommand/revokeRoles', { Persons: persons, Roles: roles });
	}
}
