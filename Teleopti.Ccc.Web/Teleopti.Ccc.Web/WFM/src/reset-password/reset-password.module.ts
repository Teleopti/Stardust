import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { TranslateLoader, TranslateModule, TranslateParser, TranslateService } from '@ngx-translate/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { CustomTranslateParser, LanguageLoaderFactory } from '../app/core/translation';
import { InvalidLinkMessageComponent } from './components/invalid-link-message.component';
import { ResetPasswordFormComponent } from './components/reset-password-form/reset-password-form.component';
import { SuccessMessageComponent } from './components/success-message.component';
import { ResetPasswordPageComponent } from './pages/reset-password.component';

@NgModule({
	declarations: [
		ResetPasswordPageComponent,
		SuccessMessageComponent,
		InvalidLinkMessageComponent,
		ResetPasswordFormComponent
	],
	imports: [
		BrowserModule,
		SharedModule,
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
	entryComponents: [ResetPasswordPageComponent],
	bootstrap: [ResetPasswordPageComponent],
	providers: []
})
export class ResetPasswordModule {
	constructor(private translate: TranslateService) {
		this.translate.setDefaultLang(navigator.language);
	}
}
