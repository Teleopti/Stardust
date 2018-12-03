import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

export interface UserPreferences {
	readonly Id: string;
	readonly UserName: string;
	readonly Language: string;
	readonly IsTeleoptiApplicationLogon: boolean;
	readonly DateFormatLocale: string;
}

@Injectable()
export class UserService {
	private _preferences$ = new ReplaySubject<UserPreferences>(1);

	public get preferences$(): Observable<UserPreferences> {
		return this._preferences$;
	}

	constructor(private http: HttpClient) {
		this.http.get('../api/Global/User/CurrentUser').subscribe({
			next: (preferences: UserPreferences) => {
				this._preferences$.next(preferences);
			}
		});
	}
}
