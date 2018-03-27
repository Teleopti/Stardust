import { Injectable } from '@angular/core';
import { Person, Role } from '../types';
import { SearchService } from './search.service';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable()
export class WorkspaceService {
	protected _people$ = new BehaviorSubject<Array<Person>>([]);

	public readonly people$: Observable<Array<Person>> = this._people$.asObservable();

	constructor(private searchService: SearchService) {}

	public async update() {
		const PersonIdList = this._people$.getValue().map(p => p.Id);
		const people = await this.searchService.getPersons({ PersonIdList });
		this._people$.next(people);
	}

	public getSelectedPerson(id: string): Person {
		return this._people$.getValue().find(p => p.Id === id);
	}

	public getSelectedPeople(): BehaviorSubject<Array<Person>> {
		return this._people$;
	}

	public isPersonSelected(id: string): boolean {
		return this._people$.getValue().findIndex(p => p.Id === id) !== -1;
	}

	public isAnySelected(): boolean {
		return this._people$.getValue().length > 0;
	}

	public selectPerson(person: Person): void {
		if (!this._people$.getValue().find(p => p.Id === person.Id)) {
			this._people$.next([...this._people$.getValue(), person]);
		}
	}

	public deselectPerson(person: Person): void {
		if (this._people$.getValue().find(p => p.Id === person.Id)) {
			this._people$.next(this._people$.getValue().filter(p => p.Id !== person.Id));
		}
	}
}
