import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ExternalApplicationService } from '../../services';
import { ExternalApplication } from '../../types';

@Injectable()
export class ListPageService {
	constructor(public externalApplicationService: ExternalApplicationService) {}

	listExternalApplications(): Observable<ExternalApplication[]> {
		return this.externalApplicationService.getExternalApplications();
	}

	revokeApiAccess(id: any): Observable<string> {
		return this.externalApplicationService.revokeApiAccess(id);
	}
}
