import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { PlanningGroupService, PlanningPeriodService } from './shared';
import { IStateService } from 'angular-ui-router';
import { TitleBarComponent } from './components/title-bar';
import { PlanningGroupsOverviewComponent } from './pages/planning-groups/planning-groups-overview.component';
import { PlanningPeriodOverviewComponent } from './pages/planning-period/planning-period-overview.component';
import {MomentModule} from "ngx-moment";
import {HeatMapColorHelper} from "./shared/heatmapcolor.service";

@NgModule({
	declarations: [TitleBarComponent, PlanningGroupsOverviewComponent, PlanningPeriodOverviewComponent],
	imports: [
		SharedModule, 
		TranslateModule.forChild(),
		MomentModule
	],
	providers: [
		PlanningGroupService,
		PlanningPeriodService,
		HeatMapColorHelper,
		{
			provide: '$state',
			useFactory: (i: any): IStateService => i.get('$state'),
			deps: ['$injector']
		}
	],
	exports: [],
	entryComponents: [PlanningGroupsOverviewComponent, PlanningPeriodOverviewComponent]
})
export class PlansModule {}

export const plansComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2Plans', ng2Component: PlanningGroupsOverviewComponent },
	{ ng1Name: 'ng2PlansPlanningPeriodOverview', ng2Component: PlanningPeriodOverviewComponent }
];
