import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { IdentityLogonService, WorkspaceService, LogonInfoFromGuidsResponse } from '../../services';
import { map, switchMap, filter, flatMap } from 'rxjs/operators';
import { of } from 'rxjs/observable/of';

export interface PersonWithIdentityLogon {
	Id: string;
	FullName?: string;
	Identity: string;
}

export interface PeopleWithIdentityLogon extends Array<PersonWithIdentityLogon> {}

@Injectable()
export class IdentityLogonPageService {
	constructor(public identityLogonService: IdentityLogonService, public workspaceService: WorkspaceService) {}

	public people$: Observable<PeopleWithIdentityLogon> = this.workspaceService.people$.pipe(
		map(people => this.mapPeopleWithFullName(people)),
		flatMap(peopleWithName => this.joinPeopleWithIdentityLogon(peopleWithName))
	);

	public save(people: PeopleWithIdentityLogon) {
		return this.identityLogonService.persistLogonNames(
			people.map(person => ({
				Identity: person.Identity,
				PersonId: person.Id
			}))
		);
	}

	private joinPeopleWithIdentityLogon(peopleWithName) {
		if (peopleWithName.length === 0) return of([]);
		const peopleIds = peopleWithName.map(person => person.Id);
		return this.identityLogonService.getLogonInfo(peopleIds).pipe(
			map((peopleWithLogon: LogonInfoFromGuidsResponse) => {
				return peopleWithName.map(personWithName => {
					const personWithLogon = peopleWithLogon.find(p => p.PersonId === personWithName.Id);
					let Identity = (personWithLogon && personWithLogon.LogonName) || '';
					return {
						...personWithName,
						Identity
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
