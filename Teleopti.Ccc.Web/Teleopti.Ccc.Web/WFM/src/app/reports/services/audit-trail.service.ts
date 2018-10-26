import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { flatMap, map, switchMap } from 'rxjs/operators';
import { Person, AuditEntry } from '../../shared/types';

export interface PersonByKeyWordResponse {
	Persons: Person[];
}

export interface AuditTrailsResponse {
	AuditEntries: AuditEntry[];
}

@Injectable()
export class AuditTrailService {
	constructor(private http: HttpClient) {}

	getPersonByKeyword(keyword: string): Observable<PersonByKeyWordResponse> {
		const response = this.http.get('../api/Search/FindPersonsByKeywords?keywords=' + keyword);

		return response as Observable<PersonByKeyWordResponse>;
	}

	getStaffingAuditTrail(personId: string, startDate: string, endDate: string): Observable<AuditTrailsResponse> {
		const response = this.http.get(
			'../api/Reports/getauditlogs?personId=' + personId + '&startDate=' + startDate + '&endDate=' + endDate
		);
		return response as Observable<AuditTrailsResponse>;
	}
}
