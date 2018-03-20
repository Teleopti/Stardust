import { Role } from './Role';

export interface Person {
	Id: string;
	FirstName?: string;
	LastName?: string;
	Site?: string;
	Team?: string;
	ApplicationLogon?: string;
	WindowsLogon?: string;
	Roles?: Array<Role>;
}
