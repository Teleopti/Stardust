import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { PlanningGroupService } from './shared';
import { IStateService } from 'angular-ui-router';
import { TitleBarComponent } from './components/title-bar';
import {PlanningGroupsOverviewComponent} from "./pages/planning-groups/planning-groups-overview.component";

@NgModule({
	declarations: [PlanningGroupsOverviewComponent, TitleBarComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [
		PlanningGroupService,
		{
			provide: '$state',
			useFactory: (i: any): IStateService => i.get('$state'),
			deps: ['$injector']
		}
	],
	exports: [],
	entryComponents: [PlanningGroupsOverviewComponent]
})
export class PlansModule {}

export const plansComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2Plans', ng2Component: PlanningGroupsOverviewComponent }
];
