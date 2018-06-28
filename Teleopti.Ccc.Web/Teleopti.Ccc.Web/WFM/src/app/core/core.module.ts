import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';
import { AuthenticatedInterceptor } from './interceptors';
import { TogglesService, UserPreferences, UserService } from './services';
import { LanguageLoaderFactory } from './translation';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService,
		UserService,
		{
			provide: HTTP_INTERCEPTORS,
			useClass: AuthenticatedInterceptor,
			multi: true
		}
	],
	entryComponents: []
})
export class CoreModule {}
