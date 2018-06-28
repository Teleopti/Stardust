import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

class LanguageLoader implements TranslateLoader {
	constructor(private http: HttpClient, private url: string) {}

	fixParamsToAngular(translations: object) {
		const replaceParam = (string, param) => string.replace(param, `{${param}}`);
		return Object.keys(translations)
			.filter(key => translations.hasOwnProperty(key))
			.reduce((trans, key) => {
				const params = translations[key].match(/\{\d+\}/) || [];
				const value = params.reduce(replaceParam, translations[key]);
				return {
					...trans,
					[key]: value
				};
			}, {});
	}

	getTranslation(lang: string): Observable<object> {
		return this.http.get(`${this.url}${lang}`).pipe(map(translations => this.fixParamsToAngular(translations)));
	}
}

export function LanguageLoaderFactory(http: HttpClient) {
	return new LanguageLoader(http, '/TeleoptiWFM/Web/api/Global/Language?lang=');
}
