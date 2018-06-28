import { NgModule } from '@angular/core';
import { MockTranslationModule, MockTranslationPipe, MockTranslateParamsDirective } from '../../mocks/translation';
import { PeopleModule } from './people.module';
import { fakeBackendProvider } from './services';

@NgModule({
	imports: [PeopleModule],
	providers: [fakeBackendProvider]
})
export class PeopleTestModule {}
