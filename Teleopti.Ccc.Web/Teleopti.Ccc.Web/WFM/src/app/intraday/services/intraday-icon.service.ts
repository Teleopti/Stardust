import { Injectable } from '@angular/core';
import { Skill } from '../types';

@Injectable()
export class IntradayIconService {
	getIcon(skill: Skill) {
		if (!skill.DoDisplayData) {
			return 'warning';
		}

		if (skill.IsMultisiteSkill) {
			return 'deployment-unit';
		}

		switch (skill.SkillType) {
			case 'SkillTypeChat':
				return 'message';
			case 'SkillTypeEmail':
				return 'mail';
			case 'SkillTypeInboundTelephony':
				return 'phone';
			case 'SkillTypeRetail':
				return 'shop';
			case 'SkillTypeBackoffice':
				return 'inbox';
			case 'SkillTypeProject':
				return 'mdi mdi-clock-fast';
			case 'SkillTypeFax':
				return 'file-text';
			case 'SkillTypeTime':
				return 'clock-circle';
			default:
				return 'exclamation-circle';
		}
	}
}
