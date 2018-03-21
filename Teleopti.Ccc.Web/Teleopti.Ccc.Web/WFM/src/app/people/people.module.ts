import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { SharedModule } from '../shared/shared.module';

import {
	MatInputModule,
	MatDialogModule,
	MatProgressSpinnerModule,
	MatButtonModule,
	MatDialog,
	MatDialogRef,
	MatCheckboxModule,
	MatPaginatorModule,
	MatDividerModule,
	MatTableModule,
	MatSortModule
} from '@angular/material';
import {
	RolePage,
	ChipComponent,
	ChipAddComponent,
	ChipRemoveComponent,
	GrantPageComponent,
	RevokePageComponent,
	SearchPageComponent,
	WorkspaceComponent,
	TitleBarComponent,
	AppLogonPageComponent,
	PageContainerComponent
} from './components';
import { RolesService, WorkspaceService, SearchService, NavigationService } from './services';
import { FormFieldInputComponent } from './components/form-field-input/form-field-input.component';

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
	providers: [WorkspaceService, RolesService, SearchService, NavigationService],
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
