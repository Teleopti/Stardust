import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable ,  of } from 'rxjs';
import { map, switchMap, filter, flatMap } from 'rxjs/operators';
import { ExternalApplicationService } from '../../services';
import { ApiAccessToken } from '../../types';

@Injectable()
export class AddAppPageService {
	constructor(private externalApplicationService: ExternalApplicationService) {}
	
	save(name: string): Observable<ApiAccessToken> {
		return this.externalApplicationService.grantApiAccess(name);
	}
}
