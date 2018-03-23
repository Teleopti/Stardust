import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { UpgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { AppComponent } from './app.component';
import { PeopleModule } from './people/people.module';
import { HttpClientModule } from '@angular/common/http';
import { CoreModule } from './core/core.module';

@NgModule({
	declarations: [AppComponent],
	imports: [CoreModule, BrowserModule, UpgradeModule, PeopleModule, HttpClientModule],
	entryComponents: [AppComponent]
})
export class AppModule {
	constructor(private upgrade: UpgradeModule) {}
	ngDoBootstrap() {}
}
