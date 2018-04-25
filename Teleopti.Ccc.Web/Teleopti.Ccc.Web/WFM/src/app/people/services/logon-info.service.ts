import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

interface PersonApplicationLogonModel {
	ApplicationLogonName: string;
	PersonId: string;
}

interface PersonIdentityLogonModel {
	Identity: string;
	PersonId: string;
}

interface PersistApplicationLogonNamesQuery {
	People: PersonApplicationLogonModel[];
}

interface PersistIdentityLogonNamesQuery {
	People: PersonIdentityLogonModel[];
}

export interface LogonInfo {
	PersonId: string;
	LogonName: string;
	Identity: string;
}

export interface LogonInfoFromGuidsResponse extends Array<LogonInfo> {}

@Injectable()
export class LogonInfoService {
	constructor(private http: HttpClient) {}

	persistAppLogonNames(people: PersonApplicationLogonModel[]) {
		const body: PersistApplicationLogonNamesQuery = {
			People: people
		};
		return this.http.post('../PersonInfo/PersistApplicationLogonNames', body);
	}

	persistIdentityLogonNames(people: PersonIdentityLogonModel[]) {
		const body: PersistIdentityLogonNamesQuery = {
			People: people
		};
		return this.http.post('../PersonInfo/PersistIdentities', body);
	}

	getLogonInfo(personIdsToGet: string[]): Observable<LogonInfoFromGuidsResponse> {
		return this.http.post('../PersonInfo/LogonInfoFromGuids', personIdsToGet) as Observable<
			LogonInfoFromGuidsResponse
		>;
	}

	appLogonExists(logon: string): Observable<boolean> {
		const params = new HttpParams().set('LogonName', logon);
		var response = this.http
			.get('../PersonInfo/LogonFromName', {
				params
			})
			.pipe(
				map((person: Person) => {
					return person !== null;
				})
			);

		return response as Observable<boolean>;
	}

	identityLogonExists(logon: string): Observable<boolean> {
		const params = new HttpParams().set('Identity', logon);
		var response = this.http
			.get('../PersonInfo/LogonFromIdentity', {
				params
			})
			.pipe(
				map((person: Person) => {
					return person !== null;
				})
			);

		return response as Observable<boolean>;
	}
}
