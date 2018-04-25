import { NgModule } from '@angular/core';
import {
	MatButtonModule,
	MatCheckboxModule,
	MatDialogModule,
	MatDividerModule,
	MatInputModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatSortModule,
	MatTableModule,
	MatListModule,
	MatGridListModule,
	MatDatepickerModule,
	MatNativeDateModule
} from '@angular/material';

import { SharedModule } from '../shared/shared.module';
import {
	AppLogonPageComponent,
	ChipAddComponent,
	ChipComponent,
	ChipRemoveComponent,
	GrantPageComponent,
	PageContainerComponent,
	RevokePageComponent,
	RolePage,
	SearchPageComponent,
	TitleBarComponent,
	WorkspaceComponent,
	PlaygroundComponent,
	IdentityLogonPageComponent
} from './components';
import {
	NavigationService,
	RolesService,
	SearchService,
	WorkspaceService,
	LogonInfoService,
	SearchOverridesService
} from './services';

@NgModule({
	declarations: [
		GrantPageComponent,
		RevokePageComponent,
		RolePage,
		ChipComponent,
		ChipAddComponent,
		ChipRemoveComponent,
		SearchPageComponent,
		WorkspaceComponent,
		TitleBarComponent,
		PageContainerComponent,
		AppLogonPageComponent,
		IdentityLogonPageComponent,
		PlaygroundComponent
	],
	imports: [
		SharedModule,
		MatCheckboxModule,
		MatDialogModule,
		MatPaginatorModule,
		MatButtonModule,
		MatDividerModule,
		MatTableModule,
		MatSortModule,
		MatProgressSpinnerModule,
		MatInputModule,
		MatListModule,
		MatGridListModule,
		MatDatepickerModule,
		MatNativeDateModule
	],
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
		PlaygroundComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
