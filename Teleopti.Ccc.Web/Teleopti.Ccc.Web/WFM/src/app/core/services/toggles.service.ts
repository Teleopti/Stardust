import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

export interface Toggles {
	readonly Wfm_PeopleWeb_PrepareForRelease_47766?: boolean;
	readonly [propName: string]: boolean;
}

@Injectable()
export class TogglesService {
	private toggles$ = new ReplaySubject<Toggles>(1);

	constructor(private http: HttpClient) {
		this.http.get('../ToggleHandler/AllToggles').subscribe({
			next: (toggles: Toggles) => {
				this.toggles$.next(toggles);
			}
		});
	}

	getToggles(): Observable<Toggles> {
		return this.toggles$;
	}
}
