import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { WorkspaceComponent } from './components';

@NgModule({
	declarations: [WorkspaceComponent],
	imports: [],
	providers: [],
	exports: [],
	entryComponents: [WorkspaceComponent]
})
export class PmModule {
	ngDoBootstrap() {}
}

export const pmComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2PmWorkspacePage', ng2Component: WorkspaceComponent }
];

export function pmRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	$urlRouterProvider.when('/pm', '/pm/workspace');
	$stateProvider
		.state('pm', {
			url: '/pm',
			template: '<div ui-view="content"></div>'
		})
		.state('pm.workspace', {
			url: '/workspace',
			views: {
				content: { template: '<ng2-pm-workspace-page></ng2-pm-workspace-page>' }
			}
		});
}
