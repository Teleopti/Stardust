import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

export interface Toggles {
	readonly Wfm_PeopleWeb_PrepareForRelease_47766?: boolean;
	readonly Wfm_Authentication_ChangePasswordMenu_76666?: boolean;
	readonly Wfm_PeopleWeb_Improve_Search_81681?: boolean;
	readonly [propName: string]: boolean;
}

@Injectable()
export class TogglesService {
	private _toggles$ = new ReplaySubject<Toggles>(1);
	public get toggles$(): Observable<Toggles> {
		return this._toggles$;
	}

	constructor(private http: HttpClient) {
		this.http.get('../ToggleHandler/AllToggles').subscribe({
			next: (toggles: Toggles) => {
				this._toggles$.next(toggles);
			}
		});
	}
}
