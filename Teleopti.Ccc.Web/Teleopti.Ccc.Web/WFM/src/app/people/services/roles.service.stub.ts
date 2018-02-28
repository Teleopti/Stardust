import { RolesService } from './roles.service';

export class RolesServiceStub extends RolesService {
	async getRoles() {
		return [
			{
				CanBeChangedByCurrentUser: true,
				Id: 'id1',
				Name: 'Role 1'
			},
			{
				CanBeChangedByCurrentUser: true,
				Id: 'id2',
				Name: 'Role 2'
			},
			{
				CanBeChangedByCurrentUser: true,
				Id: 'id3',
				Name: 'Role 3'
			},
			{
				CanBeChangedByCurrentUser: true,
				Id: 'id4',
				Name: 'Role 4'
			}
		];
	}
}
