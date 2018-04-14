import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

interface PersonApplicationLogonModel {
	ApplicationLogonName: string;
	PersonId: string;
}

interface PersistApplicationLogonNamesQuery {
	People: PersonApplicationLogonModel[];
}

export interface LogonInfo {
	PersonId: string;
	LogonName: string;
	Identity: string;
}

export interface LogonInfoFromGuidsResponse extends Array<LogonInfo> {}

@Injectable()
export class AppLogonService {
	constructor(private http: HttpClient) {}

	persistLogonNames(people: PersonApplicationLogonModel[]) {
		const body: PersistApplicationLogonNamesQuery = {
			People: people
		};
		return this.http.post('../PersonInfo/PersistApplicationLogonNames', body);
	}

	getLogonInfo(personIdsToGet: string[]): Observable<LogonInfoFromGuidsResponse> {
		return this.http.post('../PersonInfo/LogonInfoFromGuids', personIdsToGet) as Observable<
			LogonInfoFromGuidsResponse
		>;
	}

	logonNameExists(logonName: string): Observable<boolean> {
		const params = new HttpParams().set('LogonName', logonName);
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
}
