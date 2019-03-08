import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Area {
	Name?: string;
	InternalName?: string;
	icon?: string;
	url?: string;
}

@Injectable()
export class AreaService {
	private _areas$ = new ReplaySubject<Area[]>();
	public get areas$(): Observable<Area[]> {
		return this._areas$;
	}

	constructor(@Inject('$state') private $state: IStateService, private http: HttpClient) {
		this.getAreas();
	}

	private getAreas() {
		const areas$ = this.http.get('../api/Global/Application/WfmAreasWithPermission') as Observable<Area[]>;
		areas$.pipe(map(areas => areas.map(area => this.joinAreaWithRouteConfig(area)))).subscribe({
			next: (areas: Area[]) => {
				this._areas$.next(areas);
			}
		});
	}

	joinAreaWithRouteConfig(area: Area): Area {
		const state = this.$state.get(area.InternalName);
		if (!state) return area;
		const url = this.$state.href(area.InternalName) || this.$state.href(`${area.InternalName}.index`);
		if (url === null) console.warn('Could not get url for', area);
		return { ...area, url };
	}
}
