import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { flatMap, map, switchMap } from 'rxjs/operators';
import { Person } from '../types';

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

export interface PersistResponse {
	success: boolean;
	result: Array<any>;
	errors: Array<{ personId: string; message: string }>;
}

const parseError = () => <T>(source: Observable<PersistResponse>) =>
	source.pipe(
		flatMap((res: PersistResponse) => {
			if (res.success === true) return of(res);
			else if (res.success === false) return throwError(res);
		})
	);

@Injectable()
export class LogonInfoService {
	constructor(private http: HttpClient) {}

	persistAppLogonNames(people: PersonApplicationLogonModel[]) {
		const body: PersistApplicationLogonNamesQuery = {
			People: people
		};

		return this.http.post('../api/Global/GetTokenForPersistApplicationLogonNames', body).pipe(
			switchMap((challenge: PersistChallengeResponse) =>
				this.http.post('../PersonInfo/PersistApplicationLogonNames', challenge)
			),
			parseError()
		);
	}

	persistIdentityLogonNames(people: PersonIdentityLogonModel[]) {
		const body: PersistIdentityLogonNamesQuery = {
			People: people
		};

		return this.http.post('../api/Global/GetTokenForPersistIdentities', body).pipe(
			switchMap((challenge: PersistChallengeResponse) =>
				this.http.post('../PersonInfo/PersistIdentities', challenge)
			),
			parseError()
		);
	}

	getLogonInfo(personIdsToGet: string[]): Observable<LogonInfo[]> {
		return this.http.post('../PersonInfo/LogonInfoFromGuids', personIdsToGet) as Observable<LogonInfo[]>;
	}

	appLogonExists(logon: string): Observable<boolean> {
		const params = new HttpParams().set('LogonName', logon);
		const response = this.http
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
		const response = this.http
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
