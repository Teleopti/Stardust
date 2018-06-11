(function () {
	'use strict';

	describe('wfmI18nService', function () {
		var $locale, $q;
		var angularMoment = { changeLocale: function () { } };
		var dynamicLocaleService = { set: function () { } };
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
				$provide.service('dynamicLocaleService', function () { return dynamicLocaleService; });
			});
		});

		beforeEach(inject(function (_$locale_, _$q_) {
			$locale = _$locale_;
			$q = _$q_;
		}));

		it('should set locales', function () {
			inject(function (wfmI18nService) {
				spyOn(angularMoment, 'changeLocale');
				spyOn(i18nService, 'setCurrentLang');
				spyOn(dynamicLocaleService, 'set').and.callFake(function () {
					return {
						then: function (callback) {
							return callback();
						}
					};
				});
				var data = { Language: 'es', DateFormat: 'es', UserName: 'Ashley', DateFormatLocale: 'es', FirstDayOfWeek: 1 };
				wfmI18nService.setLocales(data);

				expect(angularMoment.changeLocale).toHaveBeenCalledWith('es', Object({ week: Object({ dow: 1 }) }));
				expect(i18nService.setCurrentLang).toHaveBeenCalled();
				expect(dynamicLocaleService.set).toHaveBeenCalledWith('es');
				expect($locale.DATETIME_FORMATS.FIRSTDAYOFWEEK).toBe(0);

			});
		});

	});
})();