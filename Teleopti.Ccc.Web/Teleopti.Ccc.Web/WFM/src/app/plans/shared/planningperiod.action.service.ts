import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class PlanningPeriodActionService {
	constructor(private httpClient: HttpClient) {}

	public launchScheduling(planningPeriodId: string) {
		return this.httpClient.post(
			'../api/resourceplanner/planningperiod/' + planningPeriodId + '/schedule',
			{}
		) as Observable<string>;
	}

	public optimizeIntraday(planningPeriodId: string) {
		return this.httpClient.post(
			'../api/resourceplanner/planningperiod/' + planningPeriodId + '/optimizeintraday',
			{}
		);
	}

	public clearSchedule(planningPeriodId: string) {
		return this.httpClient.delete(
			'../api/resourceplanner/planningperiod/' + planningPeriodId + '/schedule',
			{}
		) as Observable<string>;
	}

	public publishSchedule(planningPeriodId: string) {
		return this.httpClient.post('../api/resourceplanner/planningperiod/' + planningPeriodId + '/publish', {});
	}
}
