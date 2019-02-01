import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PlanningGroup } from '../models';
import { Observable } from 'rxjs';

@Injectable()
export class PlanningGroupService {
	constructor(private httpClient: HttpClient) {}

	public getPlanningGroups(): Observable<PlanningGroup[]> {
		return this.httpClient.get('../api/resourceplanner/planninggroup/') as Observable<PlanningGroup[]>;
	}

	public getPlanningGroup(groupId: string): Observable<PlanningGroup> {
		return this.httpClient.get('../api/resourceplanner/planninggroup/'+groupId) as Observable<PlanningGroup>;
	}
}
