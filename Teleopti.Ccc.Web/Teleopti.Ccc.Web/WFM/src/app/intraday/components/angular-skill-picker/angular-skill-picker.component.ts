import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { SkillPickerService } from '../../services/skill-picker.service';
import { SkillPickerItem } from '../../types';

@Component({
	selector: 'app-angular-skill-picker',
	templateUrl: './angular-skill-picker.html',
	styleUrls: ['./angular-skill-picker.component.scss']
})
export class AngularSkillPickerComponent implements OnInit {
	constructor(public skillPickerService: SkillPickerService) {
		this.filteredItems = this.skillPickerItems;
	}

	@Output()
	selected = new EventEmitter<SkillPickerItem>();

	@Output()
	hasEditPermission: boolean;

	skillPickerFilter = 'all';
	filteredItems: SkillPickerItem[] = [];
	inputValue: string;
	skillPickerItems: SkillPickerItem[] = [];

	@Input()
	selectedItem: SkillPickerItem;

	ngOnInit() {
		this.skillPickerService.getSkillsAndGroups().subscribe(item => {
			this.hasEditPermission = this.skillPickerService.hasEditPermission;
			this.skillPickerItems = item;
			this.filteredItems = item;
		});
	}

	onFilterChange() {
		if (this.skillPickerFilter === 'All') {
			this.filteredItems = this.skillPickerItems;
		} else {
			this.filteredItems = this.skillPickerItems.filter(item => {
				return item.Type.toString() === this.skillPickerFilter;
			});
		}
	}

	itemSelected(e: SkillPickerItem) {
		this.selected.emit(e);
	}

	compareFn = (o1: any, o2: any) => (o1 && o2 ? o1.Id === o2.Id : o1 === o2);
}
