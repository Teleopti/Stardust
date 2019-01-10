import { registerLocaleData } from '@angular/common';
import ar from '@angular/common/locales/ar';
import cs from '@angular/common/locales/cs';
import de from '@angular/common/locales/de';
import en from '@angular/common/locales/en';
import es from '@angular/common/locales/es';
import fa from '@angular/common/locales/fa';
import fi from '@angular/common/locales/fi';
import fr from '@angular/common/locales/fr';
import it from '@angular/common/locales/it';
import ja from '@angular/common/locales/ja';
import nb from '@angular/common/locales/nb';
import nl from '@angular/common/locales/nl';
import pl from '@angular/common/locales/pl';
import pt from '@angular/common/locales/pt';
import ru from '@angular/common/locales/ru';
import sk from '@angular/common/locales/sk';
import sv from '@angular/common/locales/sv';
import th from '@angular/common/locales/th';
import tr from '@angular/common/locales/tr';
import vi from '@angular/common/locales/vi';
import zh from '@angular/common/locales/zh';
import { Injectable } from '@angular/core';
import {
	ar_EG,
	de_DE,
	en_GB,
	es_ES,
	fa_IR,
	fi_FI,
	fr_FR,
	it_IT,
	ja_JP,
	nb_NO,
	nl_NL,
	NzI18nService,
	pl_PL,
	pt_PT,
	ru_RU,
	sk_SK,
	sv_SE,
	th_TH,
	tr_TR,
	vi_VN,
	zh_CN,
	zh_TW
} from 'ng-zorro-antd';

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
				break;
			case 'sv-SE':
				registerLocaleData(sv);
				break;
			case 'ar-EG':
				registerLocaleData(ar);
				break;
			case 'cs-CZ':
				registerLocaleData(cs);
				break;
			case 'de-DE':
				registerLocaleData(de);
				break;
			case 'es-ES':
				registerLocaleData(es);
				break;
			case 'fa-IR':
				registerLocaleData(fa);
				break;
			case 'fi-FI':
				registerLocaleData(fi);
				break;
			case 'fr-FR':
				registerLocaleData(fr);
				break;
			case 'it-IT':
				registerLocaleData(it);
				break;
			case 'ja-JP':
				registerLocaleData(ja);
				break;
			case 'nl-NL':
				registerLocaleData(nl);
				break;
			case 'nb-NO':
				registerLocaleData(nb);
				break;
			case 'pl-PL':
				registerLocaleData(pl);
				break;
			case 'pt-PT':
				registerLocaleData(pt);
				break;
			case 'ru-RU':
				registerLocaleData(ru);
				break;
			case 'sk-SK':
				registerLocaleData(sk);
				break;
			case 'th-TH':
				registerLocaleData(th);
				break;
			case 'tr-TR':
				registerLocaleData(tr);
				break;
			case 'vi-VN':
				registerLocaleData(vi);
				break;
			case 'zh-CN':
				registerLocaleData(zh);
				break;
			case 'zh-TW':
				registerLocaleData(zh);
				break;
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
