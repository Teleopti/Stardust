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
import { SettingsMenuComponent } from './components/settings-menu';
import {
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
		IdentityLogonPageComponent,
		SettingsMenuComponent
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
		TitleBarComponent,
		SearchPageComponent,
		GrantPageComponent,
		RevokePageComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent,
		SettingsMenuComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
