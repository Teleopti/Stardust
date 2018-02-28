import { SearchService } from './search.service';
import { Person } from '../types';

export class SearchServiceStub extends SearchService {
	protected peopleCache: Array<Person> = [
		{
			Id: 'id1',
			FirstName: 'First1',
			LastName: 'Lastname1',
			Roles: [
				{
					CanBeChangedByCurrentUser: true,
					Id: 'id1',
					Name: 'Role 1'
				},
				{
					CanBeChangedByCurrentUser: true,
					Id: 'id2',
					Name: 'Role 2'
				}
			]
		},
		{
			Id: 'id2',
			FirstName: 'First2',
			LastName: 'Lastname2',
			Roles: [
				{
					CanBeChangedByCurrentUser: true,
					Id: 'id1',
					Name: 'Role 1'
				}
			]
		},
		{
			Id: 'id3',
			FirstName: 'First3',
			LastName: 'Lastname3',
			Roles: [
				{
					CanBeChangedByCurrentUser: true,
					Id: 'id1',
					Name: 'Role 1'
				}
			]
		}
	]

	public async searchPeople() {
		return this.getPeople();
	}
}
