import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { Observable } from 'rxjs';

class LanguageLoader implements TranslateLoader {
	constructor(private http: HttpClient, private url: string) {}

	getTranslation(lang: string): Observable<object> {
		return this.http.get(`${this.url}${lang}`);
	}
}

export function LanguageLoaderFactory(http: HttpClient) {
	return new LanguageLoader(http, '/TeleoptiWFM/Web/api/Global/Language?lang=');
}
