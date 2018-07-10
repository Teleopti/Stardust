import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule, Injectable } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateService, TranslateParser } from '@ngx-translate/core';
import { ApiAccessModule } from './api-access/api-access.module';
import { CoreModule } from './core/core.module';
import { UserPreferences, UserService } from './core/services';
import { LanguageLoaderFactory, CustomTranslateParser } from './core/translation';
import { PeopleModule } from './people/people.module';

@NgModule({
	declarations: [],
	imports: [
		CoreModule,
		BrowserModule,
		UpgradeModule,
		PeopleModule,
		ApiAccessModule,
		HttpClientModule,
		TranslateModule.forRoot({
			loader: {
				provide: TranslateLoader,
				useFactory: LanguageLoaderFactory,
				deps: [HttpClient]
			},
			parser: { provide: TranslateParser, useClass: CustomTranslateParser }
		})
	],
	entryComponents: []
})
export class AppModule {
	constructor(
		private upgrade: UpgradeModule,
		private userService: UserService,
		private translate: TranslateService
	) {}

	ngDoBootstrap() {
		this.translate.setDefaultLang('en-GB');
		this.userService.getPreferences().subscribe({
			next: (preferences: UserPreferences) => {
				this.translate.use(preferences.Language);
			}
		});
	}
}
