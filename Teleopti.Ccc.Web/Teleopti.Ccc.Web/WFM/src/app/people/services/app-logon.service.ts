import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Person } from '../types';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

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
