import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { SharedModule } from '../shared/shared.module';

import { PeopleComponent } from './people.component';
import { PeopleService } from './services/people.service';
import {
	MatInputModule,
	MatDialogModule,
	MatProgressSpinnerModule,
	MatButtonModule,
	MatDialog,
	MatDialogRef,
	MatCheckboxModule,
	MatPaginatorModule,
	MatDividerModule
} from '@angular/material';
import {
	RolePage,
	ChipComponent,
	ChipAddComponent,
	ChipRemoveComponent,
	GrantPageComponent,
	RevokePageComponent
} from './components';
import { RolesService } from './services/roles.service';

@NgModule({
	declarations: [
		PeopleComponent,
		GrantPageComponent,
		RevokePageComponent,
		RolePage,
		ChipComponent,
		ChipAddComponent,
		ChipRemoveComponent
	],
	imports: [SharedModule, MatCheckboxModule, MatDialogModule, MatPaginatorModule, MatButtonModule, MatDividerModule],
	providers: [PeopleService, RolesService],
	exports: [],
	entryComponents: [PeopleComponent]
})
export class PeopleModule {
	constructor() {}
	ngDoBootstrap() {}
}

angular
	.module('wfm')
	.directive('ng2People', downgradeComponent({ component: PeopleComponent }) as angular.IDirectiveFactory);
