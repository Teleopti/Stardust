import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule, Injectable } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateService, TranslateParser } from '@ngx-translate/core';
import { ApiAccessModule } from './api-access/api-access.module';
import { CoreModule } from './core/core.module';
import { UserPreferences, UserService } from './core/services';
import { LanguageLoaderFactory, CustomTranslateParser, Zorroi18nService } from './core/translation';
import { PeopleModule } from './people/people.module';
import { RtaHistoricalOverviewModule } from '../../app/rta/rta/historical-overview/rta.historical.overview.module';

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
		}),
		RtaHistoricalOverviewModule
	],
	entryComponents: [],
	providers: [Zorroi18nService]
})
export class AppModule {
	constructor(
		private upgrade: UpgradeModule,
		private userService: UserService,
		private translate: TranslateService,
		private zorroi18n: Zorroi18nService
	) {}

	ngDoBootstrap() {
		this.translate.setDefaultLang('en-GB');
		this.userService.getPreferences().subscribe({
			next: (preferences: UserPreferences) => {
				this.translate.use(preferences.Language);
				this.zorroi18n.switchLanguage(preferences.Language);
			}
		});
	}
}
