import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import {
	AppLogonPageComponent,
	FeedbackMessageComponent,
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
import { CustomTranslateParser } from '../core/translation';

@NgModule({
	declarations: [
		GrantPageComponent,
		RevokePageComponent,
		SearchPageComponent,
		WorkspaceComponent,
		TitleBarComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent,
		FeedbackMessageComponent
	],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [
		WorkspaceService,
		RolesService,
		SearchService,
		SearchOverridesService,
		NavigationService,
		LogonInfoService
	],
	exports: [],
	entryComponents: [
		TitleBarComponent,
		SearchPageComponent,
		GrantPageComponent,
		RevokePageComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent,
		FeedbackMessageComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
