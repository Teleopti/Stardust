import { NgModule } from '@angular/core';
import { WINDOW } from '@wfm/common';
import { MediaQueryService } from './services/media-query.service';

@NgModule({
	imports: [],
	declarations: [],
	exports: [],
	providers: [{ provide: WINDOW, useValue: window }, MediaQueryService]
})
export class BrowserModule {}
