import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
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
