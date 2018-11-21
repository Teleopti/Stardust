import { Role } from '../../shared/types';

export interface Agent {
	Id: 'id1';
	Name: 'Agent';
}
export interface TeamLeader {
	Id: 'id2';
	Name: 'TeamLeader';
}
export interface Coach {
	Id: 'id2';
	Name: 'Coach';
}
export interface Operator {
	Id: 'id3';
	Name: 'Operator';
}
export interface Trainer {
	Id: 'id4';
	Name: 'Trainer';
}

export const agent: Agent = { Id: 'id1', Name: 'Agent' };
export const teamLeader: TeamLeader = { Id: 'id2', Name: 'TeamLeader' };
export const coach: Coach = { Id: 'id2', Name: 'Coach' };
export const operator: Operator = { Id: 'id3', Name: 'Operator' };
export const trainer: Trainer = { Id: 'id4', Name: 'Trainer' };

export const ROLES: Array<Role> = [agent, teamLeader, coach, operator, trainer];
