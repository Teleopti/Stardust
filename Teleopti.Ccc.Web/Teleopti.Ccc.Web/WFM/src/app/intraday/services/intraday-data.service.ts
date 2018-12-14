import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { map, shareReplay, catchError } from 'rxjs/operators';
import { IntradayStaffingData } from '../types/intraday-staffing-data';
import { IntradayTrafficData } from '../types/intraday-traffic-data';
import { IntradayPerformanceData, IntradayLatestTimeData } from '../types/intraday-performance-data';

// TODO: Send cancel to backend as the old one does! //Anders Sjöberg - 2018-12-10 10:45:17

@Injectable()
export class IntradayDataService {
	constructor(private httpClient: HttpClient) {}

	getOptions = {
		headers: new HttpHeaders({ timeout: `${2000}` })
	};

	getTrafficData(id: string, offset: number = 0): Observable<IntradayTrafficData> {
		return this.httpClient.get(`../api/intraday/monitorskillstatistics/${id}/${offset}`).pipe(
			shareReplay(),
			map((data): IntradayTrafficData => data as IntradayTrafficData)
		);
	}

	getPerformanceData(id: string, offset: number = 0): Observable<IntradayPerformanceData> {
		return this.httpClient.get(`../api/intraday/monitorskillperformance/${id}/${offset}`).pipe(
			shareReplay(),
			map((data): IntradayPerformanceData => data as IntradayPerformanceData)
		);
	}

	getStaffingData(id: string, offset: number = 0): Observable<IntradayStaffingData> {
		return this.httpClient
			.get<IntradayStaffingData>(`../api/intraday/monitorskillstaffing/${id}/${offset}`, this.getOptions)
			.pipe(
				shareReplay(),
				map((data): IntradayStaffingData => data as IntradayStaffingData)
			);
	}

	getGroupTrafficData(id: string, offset: number = 0): Observable<IntradayTrafficData> {
		return this.httpClient.get(`../api/intraday/monitorskillareastatistics/${id}/${offset}`).pipe(
			shareReplay(),
			map((data): IntradayTrafficData => data as IntradayTrafficData)
		);
	}

	getGroupPerformanceData(id: string, offset: number = 0): Observable<IntradayPerformanceData> {
		return this.httpClient.get<IntradayPerformanceData>(
			`../api/intraday/monitorskillareaperformance/${id}/${offset}`
		);
	}

	getGroupStaffingData(id: string, offset: number = 0): Observable<IntradayStaffingData> {
		return this.httpClient.get(`../api/intraday/monitorskillareastaffing/${id}/${offset}`).pipe(
			shareReplay(),
			map((data): IntradayStaffingData => data as IntradayStaffingData)
		);
	}

	getIntradayExportForSkillGroup = function(data) {
		return this.httpClient
			.post('../api/intraday/exportskillareadatatoexcel', data, {
				responseType: 'Blob',
				headers: new HttpHeaders()
					.set('Content-Type', 'application/json')
					.set(
						'Accept',
						'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
					)
			})
			.pipe(map((res: any) => new Blob([res], { type: 'application/vnd.ms-excel' })));
	};

	getIntradayExportForSkill = function(data) {
		return this.httpClient
			.post('../api/intraday/exportskilldatatoexcel', data, {
				responseType: 'Blob',
				headers: new HttpHeaders()
					.set('Content-Type', 'application/json')
					.set(
						'Accept',
						'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
					)
			})
			.pipe(map((res: any) => new Blob([res], { type: 'application/vnd.ms-excel' })));
	};

	getLatestTimeForSkill = function(id: string): Observable<IntradayLatestTimeData> {
		return this.httpClient.get(`../api/intraday/lateststatisticstimeforskill/${id}`).pipe(shareReplay());
	};

	getLatestTimeForSkillGroup = function(id: string): Observable<IntradayLatestTimeData> {
		return this.httpClient.get(`../api/intraday/lateststatisticstimeforskillarea/${id}`).pipe(shareReplay());
	};
}
