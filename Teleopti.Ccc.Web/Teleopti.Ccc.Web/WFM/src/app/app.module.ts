import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { PeopleModule } from './people/people.module';
import { ApiAccessModule } from './api-access/api-access.module';
import { HttpClientModule } from '@angular/common/http';
import { CoreModule } from './core/core.module';

@NgModule({
	declarations: [],
	imports: [CoreModule, BrowserModule, UpgradeModule, PeopleModule, ApiAccessModule, HttpClientModule],
	entryComponents: []
})
export class AppModule {
	constructor(private upgrade: UpgradeModule) {}
	ngDoBootstrap() {}
}
