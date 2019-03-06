import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IStateProvider } from 'angular-ui-router';

import { DowngradeableComponent } from '@wfm/types';
import { SharedModule } from '../shared/shared.module';
import { SystemSettingsComponent } from './pages';
import { GroupPageService } from '../shared/services/group-page-service';
import { BankHolidayCalendarComponent } from './components/bank-holiday-calendar';
import { BankHolidayCalendarEditComponent } from './components/bank-holiday-calendar-edit';
import { BankCalendarDataService } from './shared/bank-calendar-data.service';
import { BankHolidayCalendarAssignToSitesComponent } from './components/bank-holiday-calendar-assign-to-sites';

@NgModule({
	declarations: [
		SystemSettingsComponent,
		BankHolidayCalendarComponent,
		BankHolidayCalendarEditComponent,
		BankHolidayCalendarAssignToSitesComponent
	],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [BankCalendarDataService, GroupPageService],
	exports: [],
	entryComponents: [SystemSettingsComponent]
})
export class SystemSettingsModule {
	ngDoBootstrap() {}
}

export const systemSettingsComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2SystemSettingsPage', ng2Component: SystemSettingsComponent }
];

// AngularJS router configuration
export function systemSettingsRouterConfig($stateProvider: IStateProvider) {
	$stateProvider.state('systemSettings', {
		url: '/system-settings',
		template: '<ng2-system-settings-page></ng2-system-settings-page>'
	});
}
