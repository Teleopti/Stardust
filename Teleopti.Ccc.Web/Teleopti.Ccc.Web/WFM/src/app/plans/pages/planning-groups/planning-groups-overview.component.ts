import { Component } from '@angular/core';
import { NavigationService } from 'src/app/core/services';
import { FormBuilder, FormControl } from '@angular/forms';
import { PlanningGroup } from '../../models';
import { PlanningGroupService } from '../../shared';
@Component({
	selector: 'plans-groups-overview',
	templateUrl: './planning-groups-overview.component.html',
	styleUrls: ['./planning-groups-overview.component.scss'],
	providers: []
})
export class PlanningGroupsOverviewComponent {
	filterControl: FormControl = this.fb.control('');
	planningGroups: PlanningGroup[] = [];
	filteredPlanningGroups: PlanningGroup[] = [];

	sortName = 'Name';
	sortValue = 'ascend';
	sortMap = {
		Name   : 'ascend',
		AgentCount: null
	};

	constructor(
		private planningGroupService: PlanningGroupService,
		private navService: NavigationService,
		private fb: FormBuilder
	) {
		this.filterControl.valueChanges.subscribe(filterString => {
			this.search(filterString);
		});
		
		this.planningGroupService.getPlanningGroups().subscribe(groups => {
			this.planningGroups = groups;
			this.filterControl.updateValueAndValidity();
		});
	}

	public viewPlanningPeriods(group: PlanningGroup) {
		this.navService.go('resourceplanner.selectplanningperiod', { groupId: group.Id });
	}

	public editPlanningGroup(group: PlanningGroup) {
		this.navService.go('resourceplanner.editplanninggroup', { groupId: group.Id });
	}

	public createPlanningGroup() {
		this.navService.go('resourceplanner.createplanninggroup');
	}

	public clearFilter() {
		this.filterControl.setValue('');
	}
	
	public sort(key: string, value: string): void{
		this.sortName = key;
		this.sortValue = value;
		this.search(this.filterControl.value);
	}
	
	public search(filterString: string){
		const data = this.planningGroups.filter(g => g.Name.toLowerCase().includes(filterString.toLowerCase()));
		if (this.sortName && this.sortValue) {
			this.filteredPlanningGroups = data.sort((a, b) => (this.sortValue === 'ascend') ?
				(a[ this.sortName ].toLowerCase() > b[ this.sortName ].toLowerCase() ? 1 : -1) :
				(b[ this.sortName ].toLowerCase() > a[ this.sortName ].toLowerCase() ? 1 : -1));
		}else{
			this.filteredPlanningGroups = data;
		}
	}
}
