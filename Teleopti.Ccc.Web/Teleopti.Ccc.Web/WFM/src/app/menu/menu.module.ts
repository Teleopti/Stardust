import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateService } from 'angular-ui-router';
import { AuthenticationModule } from '../authentication/authentication.module';
import { BrowserModule } from '../browser/browser.module';
import { SharedModule } from '../shared/shared.module';
import { BusinessUnitSelectorComponent } from './components/businessunit-selector/businessunit-selector.component';
import { FeedbackComponent } from './components/feedback';
import { SettingsMenuComponent } from './components/settings-menu';
import { SideMenuComponent } from './components/side-menu/side-menu.component';
import { TopNavigationComponent } from './components/top-navigation';
import { AreaService } from './shared/area.service';
import { BusinessUnitService } from './shared/businessunit.service';
import { ToggleMenuService } from './shared/toggle-menu.service';

@NgModule({
	imports: [
		CommonModule,
		TranslateModule.forChild(),
		SharedModule,
		ReactiveFormsModule,
		AuthenticationModule,
		BrowserModule
	],
	declarations: [
		BusinessUnitSelectorComponent,
		SideMenuComponent,
		TopNavigationComponent,
		SettingsMenuComponent,
		FeedbackComponent
	],
	exports: [],
	providers: [
		BusinessUnitService,
		AreaService,
		{
			provide: '$state',
			useFactory: (i: any): IStateService => i.get('$state'),
			deps: ['$injector']
		},
		ToggleMenuService
	],
	entryComponents: [SideMenuComponent, TopNavigationComponent, SettingsMenuComponent]
})
export class MenuModule {}

export const menuComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2SideMenu', ng2Component: SideMenuComponent },
	{ ng1Name: 'ng2TopNavigation', ng2Component: TopNavigationComponent }
];
