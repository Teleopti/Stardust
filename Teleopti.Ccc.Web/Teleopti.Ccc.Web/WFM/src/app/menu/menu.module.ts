import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { IStateService } from 'angular-ui-router';
import { SharedModule } from '../shared/shared.module';
import { BusinessUnitSelectorComponent } from './components/businessunit-selector/businessunit-selector.component';
import { SideMenuComponent } from './components/side-menu/side-menu.component';
import { AreaService } from './shared/area.service';
import { BusinessUnitService } from './shared/businessunit.service';

@NgModule({
	imports: [CommonModule, TranslateModule.forChild(), SharedModule, ReactiveFormsModule],
	declarations: [BusinessUnitSelectorComponent, SideMenuComponent],
	exports: [],
	providers: [
		BusinessUnitService,
		AreaService,
		{
			provide: '$state',
			useFactory: (i: any): IStateService => i.get('$state'),
			deps: ['$injector']
		}
	],
	entryComponents: [BusinessUnitSelectorComponent, SideMenuComponent]
})
export class MenuModule {}

export const menuComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2BusinessUnitSelector', ng2Component: BusinessUnitSelectorComponent },
	{ ng1Name: 'ng2SideMenu', ng2Component: SideMenuComponent }
];
