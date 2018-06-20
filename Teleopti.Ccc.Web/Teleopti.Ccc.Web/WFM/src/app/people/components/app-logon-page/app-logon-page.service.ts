import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { LogonInfoService, WorkspaceService, LogonInfoFromGuidsResponse, LogonInfo } from '../../services';
import { map, switchMap, filter, flatMap } from 'rxjs/operators';

interface PersonWithName {
	Id: string;
	FullName: string;
}

export interface PersonWithLogon {
	Id: string;
	FullName?: string;
	Logon: string;
}

export interface PeopleWithLogon extends Array<PersonWithLogon> {}

@Injectable()
export class AppLogonPageService {
	constructor(public logonInfoService: LogonInfoService, public workspaceService: WorkspaceService) {}

	public people$: Observable<PeopleWithLogon> = this.workspaceService.people$.pipe(
		map(people => this.mapPeopleWithFullName(people)),
		flatMap(peopleWithName => this.joinPeopleWithAppLogon(peopleWithName))
	);

	public save(people: PeopleWithLogon) {
		return this.logonInfoService.persistAppLogonNames(
			people.map(person => ({
				ApplicationLogonName: person.Logon,
				PersonId: person.Id
			}))
		);
	}

	private joinPeopleWithAppLogon(peopleWithName: PersonWithName[]) {
		if (peopleWithName.length === 0) return of([]);
		const peopleIds = peopleWithName.map(person => person.Id);
		const peopleLogonInfo$ = this.logonInfoService.getLogonInfo(peopleIds);
		return peopleLogonInfo$.pipe(
			map(peopleLogonInfo =>
				peopleWithName.reduce((peopleWithLogon: PeopleWithLogon, personWithName: PersonWithName) => {
					const logonInfo = peopleLogonInfo.find(logonInfo => logonInfo.PersonId === personWithName.Id);
					const Logon = (logonInfo && logonInfo.LogonName) || '';
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
