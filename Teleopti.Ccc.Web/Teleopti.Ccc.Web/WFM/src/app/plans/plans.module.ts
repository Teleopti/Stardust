import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { PlanningGroupService } from './shared';
import { IStateService } from 'angular-ui-router';
import { TitleBarComponent } from './components/title-bar';
import {LandingPageComponent} from "./pages/planning-groups/overview/landing.component";

@NgModule({
	declarations: [LandingPageComponent, TitleBarComponent],
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
	entryComponents: [LandingPageComponent]
})
export class PlansModule {}

export const plansComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2PlansLandingPage', ng2Component: LandingPageComponent }
];
