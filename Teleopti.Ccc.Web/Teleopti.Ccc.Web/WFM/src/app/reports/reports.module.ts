import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { ScheduleAuditTrail, GeneralAuditTrail } from './components';
import { AuditTrailService } from './services';

@NgModule({
	declarations: [ScheduleAuditTrail],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [TranslateModule, AuditTrailService],
	exports: [],
	entryComponents: [ScheduleAuditTrail]
})
export class ReportModule {
	ngDoBootstrap() {}
}
