import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AppLogonService, WorkspaceService, LogonInfoFromGuidsResponse } from '../../services';
import { map, switchMap, filter, flatMap } from 'rxjs/operators';
import { of } from 'rxjs/observable/of';

export interface PersonWithAppLogon {
	Id: string;
	FullName: string;
	ApplicationLogon: string;
}

export interface PeopleWithAppLogon extends Array<PersonWithAppLogon> {}

@Injectable()
export class AppLogonPageService {
	constructor(public appLogonService: AppLogonService, public workspaceService: WorkspaceService) {}

	public people$: Observable<PeopleWithAppLogon> = this.workspaceService.people$.pipe(
		map(people => this.mapPeopleWithFullName(people)),
		flatMap(peopleWithName => this.joinPeopleWithAppLogon(peopleWithName))
	);

	private joinPeopleWithAppLogon(peopleWithName) {
		if (peopleWithName.length === 0) return of([]);
		const peopleIds = peopleWithName.map(person => person.Id);
		return this.appLogonService.getLogonInfo(peopleIds).pipe(
			map((peopleWithLogon: LogonInfoFromGuidsResponse) => {
				return peopleWithName.map(personWithName => {
					const personWithLogon = peopleWithLogon.find(p => p.PersonId === personWithName.Id);
					let ApplicationLogon = (personWithLogon && personWithLogon.LogonName) || '';
					return {
						...personWithName,
						ApplicationLogon
					};
				});
			})
		);
	}

	private mapPeopleWithFullName(people) {
		return people.map(person => ({
			Id: person.Id,
			FullName: `${person.FirstName} ${person.LastName}`
		}));
	}
}
