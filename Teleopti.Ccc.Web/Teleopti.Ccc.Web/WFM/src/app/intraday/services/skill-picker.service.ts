import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { Skill, SkillGroupsResponse, SkillPickerItem } from '../types';
import { SkillPickerItemType } from '../types/skill-picker-item';

@Injectable()
export class SkillPickerService {
	constructor(private http: HttpClient) {}
	hasEditPermission: boolean = false;

	getSkillsAndGroups(): Observable<SkillPickerItem[]> {
		//get response from skills API then get response from skillGroups API, generate an observable with skill picker items from that
		return this.http.get('../api/skillgroup/skills').pipe(
			switchMap((skills: Skill[]) =>
				this.http.get('../api/skillgroup/skillgroups').pipe(
					map((skillGroupsResponse: SkillGroupsResponse) => {
						this.hasEditPermission = skillGroupsResponse.HasPermissionToModifySkillArea;
						var list: Array<SkillPickerItem> = [];
						list = skills
							.map(
								(item): SkillPickerItem => {
									return {
										Id: item.Id,
										Name: item.Name,
										Type: SkillPickerItemType.Skill,
										Skills: []
									};
								}
							)
							.concat(
								skillGroupsResponse.SkillAreas.map(
									(item): SkillPickerItem => {
										return {
											Id: item.Id,
											Name: item.Name,
											Type: SkillPickerItemType.Group,
											Skills: item.Skills
										};
									}
								)
							);
						return list;
					})
				)
			)
		);
	}
}
