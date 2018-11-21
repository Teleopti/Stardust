import { Person } from '../../shared/types';
import { agent, Agent, teamLeader, TeamLeader } from './roles';

type Team1 = 'Team1';
type Team2 = 'Team2';
const team1: Team1 = 'Team1';
const team2: Team2 = 'Team2';

type Site1 = 'Site1';
type Site2 = 'Site2';
const site1: Site1 = 'Site1';
const site2: Site2 = 'Site2';

export interface Adina {
	Id: 'id1';
	FirstName: 'Adina';
	LastName: 'Oswald';
	Site: Site1;
	Team: Team1;
	Roles: [Agent, TeamLeader];
}
export interface Eva {
	Id: 'id2';
	FirstName: 'Eva';
	LastName: 'Speece';
	Site: Site1;
	Team: Team1;
	Roles: [Agent];
}
export interface Myles {
	Id: 'id3';
	FirstName: 'Myles';
	LastName: 'Miller';
	Site: Site2;
	Team: Team2;
	Roles: [Agent];
}

export const adina: Adina = {
	Id: 'id1',
	FirstName: 'Adina',
	LastName: 'Oswald',
	Site: site1,
	Team: team1,
	Roles: [agent, teamLeader]
};
export const eva: Eva = {
	Id: 'id2',
	FirstName: 'Eva',
	LastName: 'Speece',
	Site: site1,
	Team: team1,
	Roles: [agent]
};
export const myles: Myles = {
	Id: 'id3',
	FirstName: 'Myles',
	LastName: 'Miller',
	Site: site2,
	Team: team2,
	Roles: [agent]
};

export const PEOPLE: Array<Person> = [adina, eva, myles];
