import { Injectable } from '@angular/core';
import { Person, Role } from '../types';


@Injectable()
export class WorkspaceService {
	protected selectedPeople: Array<Person> = []

	constructor() { }

	public getSelectedPerson(id: string): Person {
		return this.selectedPeople.find(p => p.Id === id);
	}

	public getSelectedPeople(): Array<Person> {
		return this.selectedPeople;
	}

	public isPersonSelected(id: string): boolean {
		return this.selectedPeople.findIndex(p => p.Id === id) !== -1;
	}

	public isAnySelected(): boolean {
		return this.selectedPeople.length > 0;
	}

	public selectPerson(person: Person): void {
		if (!this.selectedPeople.find(p => p.Id === person.Id)) {
			this.selectedPeople = [...this.selectedPeople, person];
		}
	}

	public deselectPerson(person: Person): void {
		if (this.selectedPeople.find(p => p.Id === person.Id)) {
			this.selectedPeople = this.selectedPeople.filter(p => p.Id !== person.Id);
		}
	}
}
