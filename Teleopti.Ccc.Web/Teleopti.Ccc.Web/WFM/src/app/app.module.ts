import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { CoreModule } from './core/core.module';
import { PeopleModule } from './people/people.module';
import { ApiAccessModule } from './api-access/api-access.module';

@NgModule({
	declarations: [],
	imports: [CoreModule, BrowserModule, UpgradeModule, PeopleModule, ApiAccessModule, HttpClientModule],
	entryComponents: []
})
export class AppModule {
	constructor(private upgrade: UpgradeModule) {}
	ngDoBootstrap() {}
}
