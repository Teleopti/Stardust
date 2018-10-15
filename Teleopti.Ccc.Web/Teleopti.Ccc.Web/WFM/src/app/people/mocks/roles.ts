import { Role } from '../../shared/types';

export type Agent = { Id: 'id1'; Name: 'Agent' };
export type TeamLeader = { Id: 'id2'; Name: 'TeamLeader' };
export type Coach = { Id: 'id2'; Name: 'Coach' };
export type Operator = { Id: 'id3'; Name: 'Operator' };
export type Trainer = { Id: 'id4'; Name: 'Trainer' };

export const agent: Agent = { Id: 'id1', Name: 'Agent' };
export const teamLeader: TeamLeader = { Id: 'id2', Name: 'TeamLeader' };
export const coach: Coach = { Id: 'id2', Name: 'Coach' };
export const operator: Operator = { Id: 'id3', Name: 'Operator' };
export const trainer: Trainer = { Id: 'id4', Name: 'Trainer' };

export const ROLES: Array<Role> = [agent, teamLeader, coach, operator, trainer];
