import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { AngularSkillPickerComponent } from './components/angular-skill-picker/angular-skill-picker.component';
import { SkillPickerIconComponent } from './components/angular-skill-picker/skill-picker-icon.component';
import { IntradayChartComponent } from './components/chart/intraday-chart.component';
import { IntradayDateSelectorComponent } from './components/intraday-date-selector.component';
import { IntradayMainComponent } from './components/intraday-main.component';
import { SkillPickerService } from './services/skill-picker.service';
import { IntradayDataService } from './services/intraday-data.service';
import { IntradayTableComponent } from './components/intraday-table.component';
import { DowngradeableComponent } from '@wfm/types';
@NgModule({
	declarations: [
		IntradayMainComponent,
		AngularSkillPickerComponent,
		SkillPickerIconComponent,
		IntradayDateSelectorComponent,
		IntradayChartComponent,
		IntradayTableComponent
	],
	entryComponents: [IntradayMainComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [SkillPickerService, IntradayDataService]
})
export class IntradayModule {}

export const intradayComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2Intraday', ng2Component: IntradayMainComponent }
];
