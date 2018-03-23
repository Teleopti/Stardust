import { NgModule } from '@angular/core';

import { HttpClientModule } from '@angular/common/http';
import { TogglesService } from './services';
import { MAT_RIPPLE_GLOBAL_OPTIONS } from '@angular/material';
import { globalRippleConfig } from '../../themes/material-config';

@NgModule({
	imports: [HttpClientModule],
	exports: [],
	providers: [
		TogglesService
		// {
		// 	provide: MAT_RIPPLE_GLOBAL_OPTIONS,
		// 	useValue: globalRippleConfig
		// }
	],
	entryComponents: []
})
export class CoreModule {}
