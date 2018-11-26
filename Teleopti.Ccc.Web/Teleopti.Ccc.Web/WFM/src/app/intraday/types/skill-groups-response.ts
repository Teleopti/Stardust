import { SkillGroup } from './skill-group';

export interface SkillGroupsResponse {
	HasPermissionToModifySkillArea: boolean;
	SkillAreas: Array<SkillGroup>;
}
