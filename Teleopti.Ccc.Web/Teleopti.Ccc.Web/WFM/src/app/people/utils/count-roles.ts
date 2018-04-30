import { Person } from '../types';

export function countUniqueRolesFromPeople(people: Person[]) {
	return Object.keys(
		people.reduce((acc, person) => {
			person.Roles.forEach(role => {
				if (typeof acc[role.Id] !== 'number') acc[role.Id] = 1;
				else acc[role.Id]++;
			});
			return acc;
		}, {})
	).length;
}
