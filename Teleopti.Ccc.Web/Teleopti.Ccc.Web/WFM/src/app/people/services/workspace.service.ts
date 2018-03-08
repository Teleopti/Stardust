import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { SearchService } from './search.service';

@Injectable()
export class WorkspaceService {
	protected people: Array<Person> = [];

	constructor(private searchService: SearchService) {}

	public async update() {
		const PersonIdList = this.people.map(p => p.Id);
		const people = await this.searchService.getPersons({ PersonIdList });
		this.people = people;
	}

	public getSelectedPerson(id: string): Person {
		return this.people.find(p => p.Id === id);
	}

	public getSelectedPeople(): Array<Person> {
		return this.people;
	}

	public isPersonSelected(id: string): boolean {
		return this.people.findIndex(p => p.Id === id) !== -1;
	}

	public isAnySelected(): boolean {
		return this.people.length > 0;
	}

	public selectPerson(person: Person): void {
		if (!this.people.find(p => p.Id === person.Id)) {
			this.people = [...this.people, person];
		}
	}

	public deselectPerson(person: Person): void {
		if (this.people.find(p => p.Id === person.Id)) {
			this.people = this.people.filter(p => p.Id !== person.Id);
		}
	}
}
