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
	WorkspaceComponent
} from './components';
import { NavigationService, RolesService, SearchService, WorkspaceService, AppLogonService } from './services';
import { SearchOverridesService } from './services/search-overrides.service';

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
		AppLogonPageComponent
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
		AppLogonPageComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
