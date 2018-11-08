import { NgModule } from '@angular/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { SharedModule } from '../shared/shared.module';
import { AddAppPageComponent, ListPageComponent } from './components';
import { ExternalApplicationService, NavigationService } from './services';

@NgModule({
	declarations: [AddAppPageComponent, ListPageComponent],
	imports: [SharedModule],
	providers: [ExternalApplicationService, NavigationService],
	exports: [],
	entryComponents: [AddAppPageComponent, ListPageComponent]
})
export class ApiAccessModule {
	ngDoBootstrap() {}
}

export const apiAccessComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2ApiAccessAddAppPage', ng2Component: AddAppPageComponent },
	{ ng1Name: 'ng2ApiAccessListPage', ng2Component: ListPageComponent }
];

// AngularJS router configuration
export function apiAccessRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	$urlRouterProvider.when('/api-access', '/api-access/list');
	$stateProvider
		.state('apiaccess', {
			url: '/api-access',
			template: '<div ui-view="content"></div>'
		})
		.state('apiaccess.index', {
			url: '/list',
			views: {
				content: { template: '<ng2-api-access-list-page></ng2-api-access-list-page>' }
			}
		})
		.state('apiaccess.addapp', {
			url: '/add-app',
			views: {
				content: { template: '<ng2-api-access-add-app-page></ng2-api-access-add-app-page>' }
			}
		});
}
