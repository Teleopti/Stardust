import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AuthenticatedInterceptor } from './interceptors';
import { ThemeService, TogglesService, UserService } from './services';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService,
		UserService,
		ThemeService,
		{
			provide: HTTP_INTERCEPTORS,
			useClass: AuthenticatedInterceptor,
			multi: true
		}
	],
	entryComponents: []
})
export class CoreModule {}
