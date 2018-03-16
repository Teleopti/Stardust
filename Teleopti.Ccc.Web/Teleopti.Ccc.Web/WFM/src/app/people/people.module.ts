import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { SharedModule } from '../shared/shared.module';

import { PeopleComponent } from './people.component';
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
	TitleBarComponent
} from './components';
import { RolesService, WorkspaceService, SearchService, NavigationService } from './services';

@NgModule({
	declarations: [
		PeopleComponent,
		GrantPageComponent,
		RevokePageComponent,
		RolePage,
		ChipComponent,
		ChipAddComponent,
		ChipRemoveComponent,
		SearchPageComponent,
		WorkspaceComponent,
		TitleBarComponent
	],
	imports: [
		SharedModule,
		MatCheckboxModule,
		MatDialogModule,
		MatPaginatorModule,
		MatButtonModule,
		MatDividerModule,
		MatTableModule,
		HttpClientModule,
		MatSortModule
	],
	providers: [WorkspaceService, RolesService, SearchService, NavigationService],
	exports: [],
	entryComponents: [TitleBarComponent, SearchPageComponent, GrantPageComponent, RevokePageComponent]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
