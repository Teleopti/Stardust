import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';

import { SharedModule } from '../shared/shared.module';
import { SystemSettingsComponent } from './pages';
import { DowngradeableComponent } from '@wfm/types';

@NgModule({
	declarations: [SystemSettingsComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [TranslateModule],
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
