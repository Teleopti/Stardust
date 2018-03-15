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
	SearchPageComponent
} from './components';
import { RolesService, WorkspaceService, SearchService } from './services';
import { WorkspaceComponent } from './components/workspace/workspace.component';

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
		WorkspaceComponent
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
	providers: [WorkspaceService, RolesService, SearchService],
	exports: [],
	entryComponents: [PeopleComponent]
})
export class PeopleModule {
	ngDoBootstrap() {}
}
