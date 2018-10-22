import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';
import { GeneralAuditTrailComponent } from './pages';
import { AuditTrailService } from './services';
import { DowngradeableComponent } from '@wfm/types';

@NgModule({
	declarations: [GeneralAuditTrailComponent],
	imports: [SharedModule, TranslateModule.forChild()],
	providers: [TranslateModule, AuditTrailService],
	exports: [],
	entryComponents: [GeneralAuditTrailComponent]
})
export class ReportModule {
	ngDoBootstrap() {}
}

export const reportsComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2GeneralAuditTrailPage', ng2Component: GeneralAuditTrailComponent }
];
