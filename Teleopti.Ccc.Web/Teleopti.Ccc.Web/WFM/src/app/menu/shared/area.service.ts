import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { IStateService } from 'angular-ui-router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Area {
	Name?: string;
	InternalName?: string;
	icon?: string;
	url?: string;
}

@Injectable()
export class AreaService {
	constructor(@Inject('$state') private $state: IStateService, private http: HttpClient) {}

	getAreas(): Observable<Area[]> {
		const areas$ = this.http.get('../api/Global/Application/WfmAreasWithPermission') as Observable<Area[]>;
		return areas$.pipe(map(areas => areas.map(area => this.joinAreaWithRouteConfig(area))));
	}

	joinAreaWithRouteConfig(area: Area): Area {
		const state = this.$state.get(area.InternalName);
		if (!state) return area;
		const url = this.$state.href(area.InternalName) || this.$state.href(`${area.InternalName}.index`);
		if (url === null) console.warn('Could not get url for', area);
		return { ...area, url };
	}
}
