import { Component, Input } from '@angular/core';
import { SkillPickerItemType } from '../../types/skill-picker-item';

@Component({
	selector: 'skill-picker-icon',
	template: `
		<i class="anticon anticon-{{getTypeIconName()}}"></i>
	`
})
export class SkillPickerIconComponent {
	constructor() {}

	@Input()
	type: SkillPickerItemType;

	getTypeIconName() {
		if (this.type === SkillPickerItemType.Skill) return 'file-text';
		if (this.type === SkillPickerItemType.Group) return 'copy';

		return 'question';
	}
}
