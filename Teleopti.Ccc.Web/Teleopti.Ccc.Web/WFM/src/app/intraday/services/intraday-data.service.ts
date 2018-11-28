import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IntradayStaffingData } from '../types/intraday-staffing-data';
import { IntradayTrafficData } from '../types/intraday-traffic-data';
import { IntradayPerformanceData } from '../types/intraday-performance-data';
@Injectable()
export class IntradayDataService {
	constructor(private http: HttpClient) {}

	getStaffingData(id: string, offset: number = 0): Observable<IntradayStaffingData> {
		return this.http
			.get(`../api/intraday/monitorskillstaffing/${id}/${offset}`)
			.pipe(map((data): IntradayStaffingData => data as IntradayStaffingData));
	}

	getTrafficData(id: string, offset: number = 0): Observable<IntradayTrafficData> {
		return this.http
			.get(`../api/intraday/monitorskillstatistics/${id}/${offset}`)
			.pipe(map((data): IntradayTrafficData => data as IntradayTrafficData));
	}

	getPerformanceData(id: string, offset: number = 0): Observable<IntradayPerformanceData> {
		return this.http
			.get(`../api/intraday/monitorskillperformance/${id}/${offset}`)
			.pipe(map((data): IntradayPerformanceData => data as IntradayPerformanceData));
	}

	getGroupStaffingData(id: string, offset: number = 0): Observable<IntradayStaffingData> {
		return this.http
			.get(`../api/intraday/monitorskillareastaffing/${id}/${offset}`)
			.pipe(map((data): IntradayStaffingData => data as IntradayStaffingData));
	}

	getGroupTrafficData(id: string, offset: number = 0): Observable<IntradayTrafficData> {
		return this.http
			.get(`../api/intraday/monitorskillareastatistics/${id}/${offset}`)
			.pipe(map((data): IntradayTrafficData => data as IntradayTrafficData));
	}

	getGroupPerformanceData(id: string, offset: number = 0): Observable<IntradayPerformanceData> {
		return this.http
			.get(`../api/intraday/monitorskillareaperformance/${id}/${offset}`)
			.pipe(map((data): IntradayPerformanceData => data as IntradayPerformanceData));
	}

	getIntradayExportForSkillGroup = function(data) {
		return this.http
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
		return this.http
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
}
