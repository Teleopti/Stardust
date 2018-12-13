import { Injectable } from '@angular/core';
import {
	NzI18nService,
	sv_SE,
	en_GB,
	ar_EG,
	cs_CZ,
	de_DE,
	es_ES,
	fa_IR,
	fi_FI,
	fr_FR,
	it_IT,
	ja_JP,
	nl_NL,
	nb_NO,
	pl_PL,
	pt_PT,
	ru_RU,
	sk_SK,
	th_TH,
	tr_TR,
	vi_VN,
	zh_CN,
	zh_TW
} from 'ng-zorro-antd';

import { registerLocaleData } from '@angular/common';
import sv from '@angular/common/locales/sv';
import en from '@angular/common/locales/en';
import ar from '@angular/common/locales/ar';
import cs from '@angular/common/locales/cs';
import de from '@angular/common/locales/de';
import es from '@angular/common/locales/es';
import fa from '@angular/common/locales/fa';
import fi from '@angular/common/locales/fi';
import fr from '@angular/common/locales/fr';
import it from '@angular/common/locales/it';
import ja from '@angular/common/locales/ja';
import nl from '@angular/common/locales/nl';
import nb from '@angular/common/locales/nb';
import pl from '@angular/common/locales/pl';
import pt from '@angular/common/locales/pt';
import ru from '@angular/common/locales/ru';
import sk from '@angular/common/locales/sk';
import th from '@angular/common/locales/th';
import tr from '@angular/common/locales/tr';
import vi from '@angular/common/locales/vi';
import zh from '@angular/common/locales/zh';

@Injectable()
export class Zorroi18nService {
	constructor(private nzI18nService: NzI18nService) {}

	availableLanguages = {
		'en-GB': en_GB,
		'sv-SE': sv_SE,
		'ar-EG': ar_EG,
		'cs-CZ': de_DE,
		'de-DE': de_DE,
		'es-ES': es_ES,
		'fa-IR': fa_IR,
		'fi-FI': fi_FI,
		'fr-FR': fr_FR,
		'it-IT': it_IT,
		'ja-JP': ja_JP,
		'nl-NL': nl_NL,
		'nb-NO': nb_NO,
		'pl-PL': pl_PL,
		'pt-PT': pt_PT,
		'ru-RU': ru_RU,
		'sk-SK': sk_SK,
		'th-TH': th_TH,
		'tr-TR': tr_TR,
		'vi-VN': vi_VN,
		'zh-CN': zh_CN,
		'zh-TW': zh_TW
	};

	registerAngularLocale(userLanguage) {
		switch (userLanguage) {
			case 'en-GB':
				registerLocaleData(en);
			case 'sv-SE':
				registerLocaleData(sv);
			case 'ar-EG':
				registerLocaleData(ar);
			case 'cs-CZ':
				registerLocaleData(cs);
			case 'de-DE':
				registerLocaleData(de);
			case 'es-ES':
				registerLocaleData(es);
			case 'fa-IR':
				registerLocaleData(fa);
			case 'fi-FI':
				registerLocaleData(fi);
			case 'fr-FR':
				registerLocaleData(fr);
			case 'it-IT':
				registerLocaleData(it);
			case 'ja-JP':
				registerLocaleData(ja);
			case 'nl-NL':
				registerLocaleData(nl);
			case 'nb-NO':
				registerLocaleData(nb);
			case 'pl-PL':
				registerLocaleData(pl);
			case 'pt-PT':
				registerLocaleData(pt);
			case 'ru-RU':
				registerLocaleData(ru);
			case 'sk-SK':
				registerLocaleData(sk);
			case 'th-TH':
				registerLocaleData(th);
			case 'tr-TR':
				registerLocaleData(tr);
			case 'vi-VN':
				registerLocaleData(vi);
			case 'zh-CN':
				registerLocaleData(zh);
			case 'zh-TW':
				registerLocaleData(zh);
			default:
				registerLocaleData(en);
		}
	}

	switchLanguage(userLanguage) {
		let zorroLocale = en_GB;
		if (this.availableLanguages.hasOwnProperty(userLanguage)) {
			zorroLocale = this.availableLanguages[userLanguage];
		}

		this.registerAngularLocale(userLanguage);

		this.nzI18nService.setLocale(zorroLocale);
	}
}
