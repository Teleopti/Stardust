import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface BusinessUnit {
	readonly Id: string;
	readonly Name: string;
}

@Injectable()
export class BusinessUnitService {
	constructor(private http: HttpClient) {}

	getBusinessUnits(): Observable<BusinessUnit[]> {
		return this.http.get('../api/BusinessUnit') as Observable<BusinessUnit[]>;
	}

	selectBusinessUnit(buid: string) {
		sessionStorage.setItem('buid', buid);
	}

	getSelectedBusinessUnitId(): string {
		return sessionStorage.getItem('buid') || '';
	}
}
