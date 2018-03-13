import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { AppComponent } from './app.component';
import { PeopleModule } from './people/people.module';
import { MAT_RIPPLE_GLOBAL_OPTIONS } from '@angular/material';
import { globalRippleConfig } from '../themes/material-config';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
	declarations: [AppComponent],
	imports: [BrowserModule, UpgradeModule, PeopleModule, HttpClientModule],
	providers: [
		{
			provide: MAT_RIPPLE_GLOBAL_OPTIONS,
			useValue: globalRippleConfig
		}
	],
	entryComponents: [AppComponent]
})
export class AppModule {
	constructor(private upgrade: UpgradeModule) { }
	ngDoBootstrap() {
	}
}
