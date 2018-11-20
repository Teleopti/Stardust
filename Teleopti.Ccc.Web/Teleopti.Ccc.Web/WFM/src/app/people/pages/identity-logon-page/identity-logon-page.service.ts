import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { flatMap, map } from 'rxjs/operators';
import { LogonInfoService } from '../../../shared/services';
import { WorkspaceService } from '../../shared';

interface PersonWithName {
	Id: string;
	FullName: string;
}

export interface PersonWithLogon {
	Id: string;
	FullName?: string;
	Logon: string;
}

@Injectable()
export class IdentityLogonPageService {
	constructor(public logonInfo: LogonInfoService, public workspaceService: WorkspaceService) {}

	public people$: Observable<PersonWithLogon[]> = this.workspaceService.people$.pipe(
		map(people => this.mapPeopleWithFullName(people)),
		flatMap(peopleWithName => this.joinPeopleWithIdentityLogon(peopleWithName))
	);

	public save(people: PersonWithLogon[]) {
		return this.logonInfo.persistIdentityLogonNames(
			people.map(person => ({
				Identity: person.Logon,
				PersonId: person.Id
			}))
		);
	}

	private joinPeopleWithIdentityLogon(peopleWithName: PersonWithName[]) {
		if (peopleWithName.length === 0) return of([]);
		const peopleIds = peopleWithName.map(person => person.Id);
		const peopleLogonInfo$ = this.logonInfo.getLogonInfo(peopleIds);
		return peopleLogonInfo$.pipe(
			map(peopleLogonInfo =>
				peopleWithName.reduce((peopleWithLogon: PersonWithLogon[], personWithName: PersonWithName) => {
					const logonInfo = peopleLogonInfo.find(info => info.PersonId === personWithName.Id);
					const Logon = (logonInfo && logonInfo.Identity) || '';
					const personWithLogon = {
						...personWithName,
						Logon
					};
					return peopleWithLogon.concat(personWithLogon);
				}, [])
			)
		);
	}

	private mapPeopleWithFullName(people) {
		return people.map(person => ({
			Id: person.Id,
			FullName: `${person.FirstName} ${person.LastName}`
		}));
	}
}
