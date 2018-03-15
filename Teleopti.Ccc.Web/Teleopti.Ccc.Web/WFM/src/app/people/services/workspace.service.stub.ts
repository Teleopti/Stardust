import { WorkspaceService } from './workspace.service';
import { Person } from '../types';

export class WorkspaceServiceStub extends WorkspaceService {
	protected people: Array<Person> = [
		{
			Id: 'id1',
			FirstName: 'First1',
			LastName: 'Lastname1',
			Roles: [
				{
					Id: 'id1',
					Name: 'Role 1'
				},
				{
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
					Id: 'id1',
					Name: 'Role 1'
				}
			]
		}
	];
}
