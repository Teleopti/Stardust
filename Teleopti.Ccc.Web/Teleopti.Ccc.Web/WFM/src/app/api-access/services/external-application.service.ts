import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiAccessToken, ExternalApplication } from '../types';

@Injectable()
export class ExternalApplicationService {
	constructor(private http: HttpClient) {}

	private sortApplications(apps: ExternalApplication[]) {
		return apps.sort((r1, r2) => r1.Name.localeCompare(r2.Name));
	}

	getExternalApplications(): Observable<ExternalApplication[]> {
		return this.http.get('../api/token').pipe(map((apps: ExternalApplication[]) => this.sortApplications(apps)));
	}

	grantApiAccess(name: string): Observable<ApiAccessToken> {
		return this.http.post('../api/token', { name: name }).pipe(map((token: ApiAccessToken) => token));
	}

	revokeApiAccess(id: any): Observable<string> {
		return this.http.delete('../api/token/' + id, {
			responseType: 'text'
		});
	}
}
