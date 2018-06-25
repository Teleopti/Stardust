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
	MatNativeDateModule,
	MatPaginatorModule,
	MatProgressSpinnerModule,
	MatSortModule,
	MatTableModule,
	MatTooltipModule,
	MatMenuModule
} from '@angular/material';
import { SharedModule } from '../shared/shared.module';
import {
	ChipAddComponent,
	ChipComponent,
	ChipRemoveComponent,
	ApiAccessTitleBarComponent,
	ListPageComponent,
	AddAppPageComponent,
	PageContainerComponent
} from './components';
import {
	ExternalApplicationService,
	NavigationService
} from './services';

@NgModule({
	declarations: [
		ChipComponent,
		ChipAddComponent,
		ChipRemoveComponent,
		ApiAccessTitleBarComponent,
		AddAppPageComponent,
		ListPageComponent,
		PageContainerComponent
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
		ExternalApplicationService
	],
	exports: [],
	entryComponents: [
		ApiAccessTitleBarComponent,
		AddAppPageComponent,
		ListPageComponent
	]
})
export class ApiAccessModule {
	ngDoBootstrap() {}
}
