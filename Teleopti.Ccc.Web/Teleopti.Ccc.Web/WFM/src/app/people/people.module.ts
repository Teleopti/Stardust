import { NgModule } from '@angular/core';
import {
	MatButtonModule,
	MatCheckboxModule,
	MatDatepickerModule,
	MatDialogModule,
	MatDividerModule,
	MatGridListModule,
	MatInputModule,
	MatListModule,
	MatMenuModule,
	MatNativeDateModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatSortModule,
	MatTableModule,
	MatTooltipModule
} from '@angular/material';
import { SharedModule } from '../shared/shared.module';
import {
	AppLogonPageComponent,
	ChipAddComponent,
	ChipComponent,
	ChipRemoveComponent,
	GrantPageComponent,
	IdentityLogonPageComponent,
	PageContainerComponent,
	PlaygroundComponent,
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

@NgModule({
	declarations: [
		GrantPageComponent,
		RevokePageComponent,
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
		MatNativeDateModule,
		MatTooltipModule,
		MatMenuModule
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
