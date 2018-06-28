import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiAccessModule } from './api-access/api-access.module';
import { CoreModule } from './core/core.module';
import { UserPreferences, UserService } from './core/services';
import { LanguageLoaderFactory } from './core/translation';
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
			}
		})
	],
	entryComponents: []
})
export class AppModule {
	constructor(private upgrade: UpgradeModule, private userService: UserService, private translate: TranslateService) {
		translate.setDefaultLang('en-GB');
		userService.getPreferences().subscribe({
			next: (preferences: UserPreferences) => {
				translate.use(preferences.Language);
			}
		});
	}

	ngDoBootstrap() {}
}
