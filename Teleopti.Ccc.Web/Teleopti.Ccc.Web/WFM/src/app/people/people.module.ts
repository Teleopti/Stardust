import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import {
	AppLogonPageComponent,
	GrantPageComponent,
	IdentityLogonPageComponent,
	RevokePageComponent,
	SearchPageComponent,
	TitleBarComponent,
	WorkspaceComponent
} from './components';
import {
	LogonInfoService,
	NavigationService,
	RolesService,
	SearchOverridesService,
	SearchService,
	WorkspaceService
} from './services';

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
		LogonInfoService,
		TranslateModule
	],
	exports: [],
	entryComponents: [
		TitleBarComponent,
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
