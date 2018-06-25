import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map, tap, switchMap } from 'rxjs/operators';

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
interface PeopleLogonModel {
	Body: {
		People: PersonApplicationLogonModel[] | PersonIdentityLogonModel[];
		TimeStamp: string;
		Intent: string;
	};
}

interface PersistChallengeResponse {
	Body: PeopleLogonModel;
	Signature: string;
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

		return this.http
			.post('../api/Global/GetTokenForPersistApplicationLogonNames', body)
			.pipe(
				switchMap((challenge: PersistChallengeResponse) =>
					this.http.post('../PersonInfo/PersistApplicationLogonNames', challenge)
				)
			);
	}

	persistIdentityLogonNames(people: PersonIdentityLogonModel[]) {
		const body: PersistIdentityLogonNamesQuery = {
			People: people
		};

		return this.http
			.post('../api/Global/GetTokenForPersistIdentities', body)
			.pipe(
				switchMap((challenge: PersistChallengeResponse) =>
					this.http.post('../PersonInfo/PersistIdentities', challenge)
				)
			);
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
