import { NgModule } from '@angular/core';
import { fakeBackendProvider } from './services';

import { PeopleModule } from './people.module';

@NgModule({
	imports: [PeopleModule],
	providers: [fakeBackendProvider]
})
export class PeopleTestModule {}
