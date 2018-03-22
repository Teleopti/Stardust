import { Injectable } from '@angular/core';
import { Person } from '../types';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PeopleOverride {
	people: Person[];
	fields: string[];
}

@Injectable()
export class SearchOverridesService {
	private peopleOverrides$ = new BehaviorSubject<PeopleOverride>({ fields: [], people: [] });

	public getOverrides(): BehaviorSubject<PeopleOverride> {
		return this.peopleOverrides$;
	}

	public mergeOptimistic(people: Person[], fields: string[]) {
		this.peopleOverrides$.next({
			people,
			fields
		});
	}

	public applyOverrides(people$: BehaviorSubject<Array<Person>>): Array<Person> {
		const oldPeople = people$.getValue();
		const { people, fields } = this.peopleOverrides$.getValue();
		return oldPeople.map(person => {
			const newPerson = people.find(p => p.Id === person.Id);
			if (typeof newPerson === 'undefined') return person;
			return {
				...person,
				Roles: fields.includes('Roles') ? newPerson.Roles : person.Roles
			};
		});
	}
}
