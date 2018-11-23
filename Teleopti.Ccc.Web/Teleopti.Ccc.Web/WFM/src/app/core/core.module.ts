import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { WINDOW } from '@wfm/common';
import { IStateService } from 'angular-ui-router';
import { AuthenticatedInterceptor, BusinessUnitInterceptor, ClientOutdatedInterceptor } from './interceptors';
import { NavigationService, ThemeService, TogglesService, UserService, VersionService } from './services';
import { InterceptorOverrideService } from './services/interceptor-override.service';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService,
		UserService,
		ThemeService,
		VersionService,
		InterceptorOverrideService,
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
		},
		{
			provide: '$state',
			useFactory: (i: any): IStateService => i.get('$state'),
			deps: ['$injector']
		},
		NavigationService,
		{ provide: WINDOW, useValue: window }
	],
	entryComponents: []
})
export class CoreModule {}
