import { Component, Inject } from '@angular/core';
import { PlanningGroup } from '../../models';
import { PlanningGroupService } from '../../shared';
import { NavigationService } from 'src/app/core/services';
import { Observable } from 'rxjs';
import { filter, map, debounce, debounceTime, switchMap, shareReplay } from 'rxjs/operators';
import { FormBuilder, FormGroup, FormControl } from '@angular/forms';
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
