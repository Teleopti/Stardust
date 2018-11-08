import { Skill } from './skill';

export interface SkillGroup {
	Id: string;
	Name: string;
	Skills: Array<Skill>;
}
