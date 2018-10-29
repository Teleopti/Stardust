import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AuthenticatedInterceptor, BusinessUnitInterceptor, ClientOutdatedInterceptor } from './interceptors';
import { ThemeService, TogglesService, UserService, VersionService } from './services';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService,
		UserService,
		ThemeService,
		VersionService,
		{
			provide: HTTP_INTERCEPTORS,
			useClass: AuthenticatedInterceptor,
			multi: true
		},
		{
			provide: HTTP_INTERCEPTORS,
			useClass: BusinessUnitInterceptor,
			multi: true
		},
		{
			provide: HTTP_INTERCEPTORS,
			useClass: ClientOutdatedInterceptor,
			multi: true
		}
	],
	entryComponents: []
})
export class CoreModule {}
