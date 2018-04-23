import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map, debounceTime } from 'rxjs/operators';
import { of } from 'rxjs/observable/of';

interface PersonIdentityLogonModel {
	Identity: string;
	PersonId: string;
}

interface PersistIdentityLogonNamesQuery {
	People: PersonIdentityLogonModel[];
}

interface LogonInfo {
	PersonId: string;
	LogonName: string;
	Identity: string;
}

interface LogonInfoFromGuidsResponse extends Array<LogonInfo> {}

@Injectable()
export class IdentityLogonService {
	constructor(private http: HttpClient) {}

	persistLogonNames(people: PersonIdentityLogonModel[]) {
		console.warn('Not implemented');
		return of("something")
	}

	getLogonInfo(personIdsToGet: string[]): Observable<LogonInfoFromGuidsResponse> {
		return of(
			personIdsToGet.map(id => ({
				PersonId: id,
				LogonName: `test+${id}`,
				Identity: `test+${id}`
			}))
		).pipe(debounceTime(300));
	}

	logonNameExists(logonName: string): Observable<boolean> {
		return of(false).pipe(debounceTime(300));
	}
}
