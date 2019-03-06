import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class SystemSettingsService {
	constructor(private _httpClient: HttpClient) {}

	checkPermission(): Observable<boolean> {
		return this._httpClient.get('../api/SystemSetting/HasPermission') as Observable<boolean>;
	}
}
