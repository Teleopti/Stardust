import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { AuthenticatedInterceptor } from './interceptors';
import { TogglesService } from './services';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService,
		// {
		// 	provide: MAT_RIPPLE_GLOBAL_OPTIONS,
		// 	useValue: globalRippleConfig
		// }
		{
			provide: HTTP_INTERCEPTORS,
			useClass: AuthenticatedInterceptor,
			multi: true
		}
	],
	entryComponents: []
})
export class CoreModule {}
