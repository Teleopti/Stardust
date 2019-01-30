import { Component} from '@angular/core';
import { NavigationService } from 'src/app/core/services';
import { map} from 'rxjs/operators';
import { FormBuilder, FormControl } from '@angular/forms';
import {PlanningGroup} from "../../../models";
import {PlanningGroupService} from "../../../shared";
@Component({
	selector: 'plans-landing-page',
	templateUrl: './landing.component.html',
	styleUrls: ['./landing.component.scss'],
	providers: []
})
export class LandingPageComponent {
	filterControl: FormControl = this.fb.control('');
	planningGroups: PlanningGroup[] = [];
	filteredPlanningGroups: PlanningGroup[] = [];

	constructor(
		private planningGroupService: PlanningGroupService,
		private navService: NavigationService,
		private fb: FormBuilder
	) {
		this.planningGroupService.getPlanningGroups().subscribe(groups => {
			this.planningGroups = groups;
			this.filterControl.updateValueAndValidity();
		});
		this.filterControl.valueChanges
			.pipe(
				map(filterString => {
					return this.planningGroups.filter(g => g.Name.includes(filterString));
				})
			)
			.subscribe(filteredGroups => {
				this.filteredPlanningGroups = filteredGroups;
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
}
