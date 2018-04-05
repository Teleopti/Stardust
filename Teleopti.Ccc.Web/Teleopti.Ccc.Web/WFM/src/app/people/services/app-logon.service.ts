import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

interface LogonFromNameQuery {
	LogonName: string;
}

interface LogonFromNameResponse {
	PersonId: string;
	LogonName: string;
	Identity: string;
}

interface LogonPerson {
	PersonId: string;
	ApplicationLogon: string;
}

interface PersistApplicationLogonNamesQuery {
	People: LogonPerson[];
}

interface ResListPerson {
	PersonId: string;
}

interface PersistApplicationLogonNamesResponse {
	ResultList: ResListPerson[];
}

interface LogonInfoSucess {}

@Injectable()
export class AppLogonService {
	constructor(private http: HttpClient) {}

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