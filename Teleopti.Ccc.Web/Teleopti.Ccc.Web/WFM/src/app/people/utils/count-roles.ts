import { Person } from '../../shared/types';

export function countUniqueRolesFromPeople(people: Person[]) {
	const roles = new Set();
	people.forEach(person => {
		person.Roles.forEach(role => {
			roles.add(role.Id);
		});
	});
	return roles.size;
}
