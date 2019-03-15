import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { IntradayIconService } from '../../services/intraday-icon.service';
import { SkillPickerService } from '../../services/skill-picker.service';
import { SkillPickerItem, SkillPickerItemType } from '../../types';

@Component({
	selector: 'app-angular-skill-picker',
	templateUrl: './angular-skill-picker.html',
	styleUrls: ['./angular-skill-picker.component.scss']
})
export class AngularSkillPickerComponent implements OnInit {
	constructor(public skillPickerService: SkillPickerService, public skillIcons: IntradayIconService) {
		this.skillGroups = this.skills;
	}

	@Output()
	selected = new EventEmitter<SkillPickerItem>();

	@Output()
	hasEditPermission: boolean;

	skillGroups: SkillPickerItem[] = [];
	skills: SkillPickerItem[] = [];
	inputValue: string;

	@Input()
	selectedItem: SkillPickerItem;

	ngOnInit() {
		this.skillPickerService.getSkillsAndGroups().subscribe(item => {
			this.hasEditPermission = this.skillPickerService.hasEditPermission;
			this.skills = item.filter(x => x.Type === SkillPickerItemType.Skill);
			this.skillGroups = item.filter(x => x.Type === SkillPickerItemType.Group);
		});
	}

	itemSelected(e: SkillPickerItem) {
		this.selected.emit(e);
	}

	compareFn = (o1: any, o2: any) => (o1 && o2 ? o1.Id === o2.Id : o1 === o2);
}
