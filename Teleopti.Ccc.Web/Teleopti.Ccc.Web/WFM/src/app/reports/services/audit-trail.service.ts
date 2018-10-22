import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { flatMap, map, switchMap } from 'rxjs/operators';

export interface Person {
	Id: string;
	Name: string;
}

@Injectable()
export class AuditTrailService {
	constructor(private http: HttpClient) {}

	getPersonByKeyword(keyword: string): Observable<Person> {
		const response = this.http.get('../api/Search/FindPersonsByKeywords');
		console.log(response);
		return response as Observable<Person>;
	}
	/*personsWhoChangedSchedules(): Observable<PersonsWhoChangedSchedulesResponse> {
		const response = this.http.get('../api/Reports/PersonsWhoChangedSchedules');
		return response as Observable<PersonsWhoChangedSchedulesResponse>;
	}

	getTeams({ startDate, endDate }): Observable<OrgUnitsResponse> {
		const response = this.http.post('../api/Reports/OrganizationSelectionAuditTrail', {
			startDate,
			endDate
		});
		return response as Observable<OrgUnitsResponse>;
	}

	search(body: ScheduleAuditTrailReportQuery): Observable<SearchResult> {
		const response = this.http.post('../api/Reports/ScheduleAuditTrailReport', body);
		return response as Observable<SearchResult>;
	}*/
}

/*export interface PersonWhoChangedSchedule {
	Id: string;
	Name: string;
}

export interface PersonsWhoChangedSchedulesResponse extends Array<PersonWhoChangedSchedule> {}

export interface OrgUnit {
	Children: OrgUnit[];
	Id: string;
	Name: string;
}

export interface OrgUnitsResponse extends Array<OrgUnit> {}

export interface ScheduleAuditTrailReportQuery {
	AffectedPeriodEndDate: string;
	AffectedPeriodStartDate: string;
	ChangedByPersonId: string;
	ChangesOccurredEndDate: string;
	ChangesOccurredStartDate: string;
	MaximumResults: number;
	TeamIds: string[];
}

export interface SearchResultRow {
	ModifiedAt: string;
	ModifiedBy: string;
	ScheduledAgent: string;
	ShiftType: string;
	AuditType: string;
	Detail: string;
	ScheduleStart: string;
	ScheduleEnd: string;
}
export interface SearchResult extends Array<SearchResultRow> {}

@Injectable()
export class AuditTrailService {
	constructor(private http: HttpClient) {}

	personsWhoChangedSchedules(): Observable<PersonsWhoChangedSchedulesResponse> {
		const response = this.http.get('../api/Reports/PersonsWhoChangedSchedules');
		return response as Observable<PersonsWhoChangedSchedulesResponse>;
	}

	getTeams({ startDate, endDate }): Observable<OrgUnitsResponse> {
		const response = this.http.post('../api/Reports/OrganizationSelectionAuditTrail', {
			startDate,
			endDate
		});
		return response as Observable<OrgUnitsResponse>;
	}

	search(body: ScheduleAuditTrailReportQuery): Observable<SearchResult> {
		const response = this.http.post('../api/Reports/ScheduleAuditTrailReport', body);
		return response as Observable<SearchResult>;
	}
}

*/
