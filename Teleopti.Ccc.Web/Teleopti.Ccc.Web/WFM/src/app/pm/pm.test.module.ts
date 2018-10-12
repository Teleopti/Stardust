import { NgModule } from '@angular/core';
import { MockTranslationModule, MockTranslationPipe, MockTranslateParamsDirective } from '../../mocks/translation';
import { PmModule } from './pm.module';

@NgModule({
    imports: [PmModule],
    providers: []
})
export class PmTestModule {}
