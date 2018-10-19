import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { ScheduleAuditTrailComponent, GeneralAuditTrailComponent } from './pages';
import { AuditTrailService } from './services';
import { DowngradeableComponent } from '@wfm/types';

@NgModule({
	declarations: [ScheduleAuditTrailComponent, GeneralAuditTrailComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [TranslateModule, AuditTrailService],
	exports: [],
	entryComponents: [ScheduleAuditTrailComponent, GeneralAuditTrailComponent]
})
export class ReportModule {
	ngDoBootstrap() {}
}

export const reportsComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2ScheduleAuditTrailPage', ng2Component: ScheduleAuditTrailComponent },
	{ ng1Name: 'ng2GeneralAuditTrailPage', ng2Component: GeneralAuditTrailComponent }
];
