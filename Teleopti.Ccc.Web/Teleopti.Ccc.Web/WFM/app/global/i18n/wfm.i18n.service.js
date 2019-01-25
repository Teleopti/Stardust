(function() {
	'use strict';

	angular
		.module('wfm.i18n', ['ui.grid', 'pascalprecht.translate', 'tmh.dynamicLocale'])
		.service('wfmI18nService', ['i18nService', '$translate', 'amMoment', 'tmhDynamicLocale', wfmI18nService]);

	function wfmI18nService(i18nService, $translate, angularMoment, dynamicLocaleService) {
		var service = {};
		service.setLocales = setLocales;
		return service;

		function setLocales(data) {
            angularMoment.changeLocale(data.DateFormatLocale);
			if (!data.Language)
				return;
            
			$translate.use(data.Language);
            dynamicLocaleService.set(data.DateFormatLocale);
			
			// i18nService is for UI Grid localization.
			// Languages supported by it is less than languages in server side (Refer to http://ui-grid.info/docs/#/tutorial/104_i18n).
			// Need do some primary language checking.
			var currentLang = 'en';
			var serverSideLang = data.Language.toLowerCase();
			var dashIndex = serverSideLang.indexOf('-');
			var primaryLang = dashIndex > -1 ? serverSideLang.substring(0, dashIndex) : serverSideLang;
			var langs = i18nService.getAllLangs();
			if (langs.indexOf(serverSideLang) > -1) {
				currentLang = serverSideLang;
			} else if (langs.indexOf(primaryLang) > -1) {
				currentLang = primaryLang;
			}
			i18nService.setCurrentLang(currentLang);
		}
	}
})();
