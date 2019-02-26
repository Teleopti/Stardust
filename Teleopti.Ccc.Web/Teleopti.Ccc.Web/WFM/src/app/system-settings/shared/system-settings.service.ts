import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';

@Injectable()
export class SystemSettingsService {
	constructor(private _httpClient: HttpClient) {}

	checkPermission(): Observable<boolean> {
		return this._httpClient.get('../api/SystemSetting/HasPermission') as Observable<boolean>;
	}
}
