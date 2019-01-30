import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class PlanningPeriodService {
	constructor(private httpClient: HttpClient) {}

	public lastJobResult(planningPeriodId: string): Observable<any> {
		return this.httpClient.get('../api/resourceplanner/planningperiod/'+planningPeriodId+'/result') as Observable<any>; 
	}
}
