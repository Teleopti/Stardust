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
	MatGridListModule
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
	PlaygroundComponent
} from './components';
import {
	NavigationService,
	RolesService,
	SearchService,
	WorkspaceService,
	AppLogonService,
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
		MatGridListModule
	],
	providers: [
		WorkspaceService,
		RolesService,
		SearchService,
		SearchOverridesService,
		NavigationService,
		AppLogonService
	],
	exports: [],
	entryComponents: [
		TitleBarComponent,
		SearchPageComponent,
		GrantPageComponent,
		RevokePageComponent,
		AppLogonPageComponent,
		PlaygroundComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
