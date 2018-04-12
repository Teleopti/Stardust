import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { SearchService } from './search.service';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable()
export class WorkspaceService {
	people$: BehaviorSubject<Array<Person>> = new BehaviorSubject<Array<Person>>([]);

	constructor(private searchService: SearchService) {}

	public async update() {
		const PersonIdList = this.people$.getValue().map(p => p.Id);
		const people = await this.searchService.getPersons({ PersonIdList });
		this.people$.next(people);
	}

	public getSelectedPerson(id: string): Person {
		return this.people$.getValue().find(p => p.Id === id);
	}

	public getSelectedPeople(): BehaviorSubject<Array<Person>> {
		return this.people$;
	}

	public isPersonSelected(id: string): boolean {
		return this.people$.getValue().findIndex(p => p.Id === id) !== -1;
	}

	public isAnySelected(): boolean {
		return this.people$.getValue().length > 0;
	}

	public selectPerson(person: Person): void {
		if (!this.people$.getValue().find(p => p.Id === person.Id)) {
			this.people$.next([...this.people$.getValue(), person]);
		}
	}

	public deselectPerson(person: Person): void {
		if (this.people$.getValue().find(p => p.Id === person.Id)) {
			this.people$.next(this.people$.getValue().filter(p => p.Id !== person.Id));
		}
	}
}
