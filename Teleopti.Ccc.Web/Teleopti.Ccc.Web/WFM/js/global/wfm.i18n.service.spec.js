(function () {
	'use strict';

	describe('wfmI18nService', function () {
		var angularMoment = { changeLocale: function () { } };
		var dynamicLocaleService = { set: function() {} };
		var i18nService = {
			getAllLangs: function () { return []; },
			setCurrentLang: function () { }
		};

		beforeEach(function () {
			module('wfm.i18n');
			module(function ($provide) {
				$provide.service('amMoment', function () { return angularMoment; });
				$provide.service('i18nService', function () { return i18nService; });
				$provide.service('tmhDynamicLocale', function () { return dynamicLocaleService; });
			});
		});

		it('should set locales', function () {
			inject(function (wfmI18nService) {
				spyOn(angularMoment, 'changeLocale');
				spyOn(i18nService, 'setCurrentLang');
				spyOn(dynamicLocaleService, 'set');
				var data = { Language: 'es', DateFormat: 'es', UserName: 'Ashley', DateFormatLocale: 'es' };

				wfmI18nService.setLocales(data);

				expect(angularMoment.changeLocale).toHaveBeenCalledWith('es');
				expect(dynamicLocaleService.set).toHaveBeenCalledWith('es');
				expect(i18nService.setCurrentLang).toHaveBeenCalled();
			});
		});
		
	});
})();