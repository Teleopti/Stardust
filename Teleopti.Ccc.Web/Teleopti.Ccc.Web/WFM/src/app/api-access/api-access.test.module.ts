import { NgModule } from '@angular/core';
import { ApiAccessModule } from './api-access.module';
import { fakeBackendProvider } from './services';

@NgModule({
	imports: [ApiAccessModule],
	providers: [fakeBackendProvider]
})
export class ApiAccessTestModule {}
