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
	MatTableModule
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
import { FormFieldInputComponent } from './components/form-field-input/form-field-input.component';
import { NavigationService, RolesService, SearchService, WorkspaceService } from './services';
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
		AppLogonPageComponent,
		FormFieldInputComponent
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
		MatInputModule
	],
	providers: [WorkspaceService, RolesService, SearchService, SearchOverridesService, NavigationService],
	exports: [],
	entryComponents: [
		TitleBarComponent,
		SearchPageComponent,
		GrantPageComponent,
		RevokePageComponent,
		AppLogonPageComponent,
		FormFieldInputComponent
	]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
