import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { SharedModule } from '../shared/shared.module';
import { TitleBarComponent, WorkspaceComponent } from './components';
import {
	AppLogonPageComponent,
	GrantPageComponent,
	IdentityLogonPageComponent,
	RevokePageComponent,
	SearchPageComponent
} from './pages';
import { NavigationService, RolesService, SearchOverridesService, SearchService, WorkspaceService } from './shared';

@NgModule({
	declarations: [
		GrantPageComponent,
		RevokePageComponent,
		SearchPageComponent,
		WorkspaceComponent,
		TitleBarComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent
	],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [
		WorkspaceService,
		RolesService,
		SearchService,
		SearchOverridesService,
		NavigationService,
		TranslateModule
	],
	exports: [],
	entryComponents: [
		SearchPageComponent,
		GrantPageComponent,
		RevokePageComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}

export const peopleComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2PeopleSearchPage', ng2Component: SearchPageComponent },
	{ ng1Name: 'ng2PeopleGrantPage', ng2Component: GrantPageComponent },
	{ ng1Name: 'ng2PeopleRevokePage', ng2Component: RevokePageComponent },
	{ ng1Name: 'ng2PeopleAppLogonPage', ng2Component: AppLogonPageComponent },
	{ ng1Name: 'ng2PeopleIdentityLogonPage', ng2Component: IdentityLogonPageComponent }
];

// AngularJS router configuration
export function peopleRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	$urlRouterProvider.when('/people', '/people/search');
	$stateProvider
		.state('people', {
			url: '/people',
			template: '<div ui-view="content"></div>'
		})
		.state('people.grant', {
			url: '/roles/grant',
			views: {
				content: { template: '<ng2-people-grant-page></ng2-people-grant-page>' }
			}
		})
		.state('people.revoke', {
			url: '/roles/revoke',
			views: {
				content: { template: '<ng2-people-revoke-page></ng2-people-revoke-page>' }
			}
		})
		.state('people.applogon', {
			url: '/access/applicationlogon',
			views: {
				content: { template: '<ng2-people-app-logon-page></ng2-people-app-logon-page>' }
			}
		})
		.state('people.identitylogon', {
			url: '/access/identitylogon',
			views: {
				content: { template: '<ng2-people-identity-logon-page></ng2-people-identity-logon-page>' }
			}
		})
		.state('people.index', {
			url: '/search',
			views: {
				content: { template: '<ng2-people-search-page></ng2-people-search-page>' }
			}
		});
}
