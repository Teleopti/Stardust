import { HttpClientModule, HttpClient } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { CoreModule } from './core/core.module';
import { PeopleModule } from './people/people.module';
import { ApiAccessModule } from './api-access/api-access.module';
import { LanguageLoaderFactory } from './core/translation';
import { UserService, UserPreferences } from './core/services';

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
			}
		})
	],
	entryComponents: []
})
export class AppModule {
	constructor(private upgrade: UpgradeModule, private userService: UserService, private translate: TranslateService) {
		translate.setDefaultLang('en');
		userService.getPreferences().subscribe({
			next: (preferences: UserPreferences) => {
				translate.use(preferences.Language);
			}
		});
	}

	ngDoBootstrap() {}
}
