import { Skill } from './skill';

export enum SkillPickerItemType {
	Skill,
	Group
}

export interface SkillPickerItem {
	Name: string;
	Type: SkillPickerItemType;
	Id: string;
	Skill?: Skill;
	Skills: Skill[];
}
