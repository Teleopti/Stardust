import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { SharedModule } from '../shared/shared.module';
import { BusinessUnitSelectorComponent } from './components/businessunit-selector/businessunit-selector.component';
import { BusinessUnitService } from './shared/businessunit.service';

@NgModule({
	imports: [CommonModule, TranslateModule.forChild(), SharedModule, ReactiveFormsModule],
	declarations: [BusinessUnitSelectorComponent],
	exports: [],
	providers: [BusinessUnitService],
	entryComponents: [BusinessUnitSelectorComponent]
})
export class MenuModule {}

export const menuComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2BusinessUnitSelector', ng2Component: BusinessUnitSelectorComponent }
];
