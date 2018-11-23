import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { UpgradeModule } from '@angular/upgrade/static';
import { TranslateLoader, TranslateModule, TranslateParser, TranslateService } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { ApiAccessModule } from './api-access/api-access.module';
import { AuthenticationModule } from './authentication/authentication.module';
import { BootstrapComponent } from './components/bootstrap/bootstrap.component';
import { CoreModule } from './core/core.module';
import { UserPreferences, UserService } from './core/services';
import { CustomTranslateParser, LanguageLoaderFactory, Zorroi18nService } from './core/translation';
import { InsightsModule } from './insights/insights.module';
import { MenuModule } from './menu/menu.module';
import { PeopleModule } from './people/people.module';
import { ReportModule } from './reports/reports.module';
import { ResetPasswordModule } from './reset-password/reset-password.module';

@NgModule({
	declarations: [BootstrapComponent],
	imports: [
		CoreModule,
		BrowserModule,
		UpgradeModule,
		PeopleModule,
		AuthenticationModule,
		ApiAccessModule,
		HttpClientModule,
		InsightsModule,
		MenuModule,
		ResetPasswordModule,
		TranslateModule.forRoot({
			loader: {
				provide: TranslateLoader,
				useFactory: LanguageLoaderFactory,
				deps: [HttpClient]
			},
			parser: { provide: TranslateParser, useClass: CustomTranslateParser }
		}),
		ReportModule
	],
	entryComponents: [BootstrapComponent],
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

export const appComponents: DowngradeableComponent[] = [{ ng1Name: 'ng2Bootstrap', ng2Component: BootstrapComponent }];
