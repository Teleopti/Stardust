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

	switchLanguage(userLanguage) {
		let zorroLocale = en_GB;
		if (this.availableLanguages.hasOwnProperty(userLanguage)) {
			zorroLocale = this.availableLanguages[userLanguage];
		}

		this.nzI18nService.setLocale(zorroLocale);
	}
}
