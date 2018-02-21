import { Role } from './Role';

export interface Person {
	Id: string;
	FirstName: string;
	LastName: string;
	Roles: Array<Role>;
}
