import { Injectable } from '@angular/core';
import { Moment } from 'moment';
import { SkillPickerItem, IntradayChartType, Skill } from '../types';

export interface IntradayPersistedData {
	selectedSkillOrGroup: SkillPickerItem;
	selectedSubSkill: Skill;
	selectedSubSkillId: string;
	selectedOffset: number;
	selectedChartType: IntradayChartType;
	selectedDate: Moment;
}

@Injectable()
export class IntradayPersistService {
	getPersisted(): IntradayPersistedData {
		return JSON.parse(localStorage.getItem('IntradayPersistedData'));
	}

	setPersisted(data: IntradayPersistedData) {
		localStorage.setItem('IntradayPersistedData', JSON.stringify(data));
	}
}
