import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

export interface UserPreferences {
	readonly UserName: string;
	readonly Language: string;
}

@Injectable()
export class UserService {
	private preferences$ = new ReplaySubject<UserPreferences>(1);

	constructor(private http: HttpClient) {
		this.http.get('../api/Global/User/CurrentUser').subscribe({
			next: (preferences: UserPreferences) => {
				this.preferences$.next(preferences);
			}
		});
	}

	getPreferences(): Observable<UserPreferences> {
		return this.preferences$;
	}
}
