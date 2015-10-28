(function() {
	'use strict';

	angular.module('currentUserInfoService', ['angularMoment', 'ngStorage', 'pascalprecht.translate', 'ui.grid'])
		.service('AuthenticationRequests', ['$injector', function ($injector) {
			var service = {};
			
			service.getCurrentUser = function () {
				var $http = $injector.get('$http');
				return $http.get('../api/Global/User/CurrentUser');
			};

			service.setBuidOnHeaders = function (buid) {
				var $http = $injector.get('$http');
				$http.defaults.headers.common['X-Business-Unit-Filter'] = buid; //should call http module
			};

			return service;
		}])
		.service('CurrentUserInfo', ['AuthenticationRequests', '$q', 'amMoment', 'i18nService', '$sessionStorage', '$translate',
			function (AuthenticationRequests, $q, angularMoment, i18nService, $sessionStorage, $translate) {
			var userName;
			var defaultTimeZone;
			var language;
			var dateFormatLocale;
			var numberFormat;
			var timeout;
			var service = {};

			service.SetCurrentUserInfo = function (data) {
				userName = data.UserName;
				defaultTimeZone = data.DefaultTimeZone;
				language = data.Language;
				dateFormatLocale = data.DateFormatLocale;
				numberFormat = data.NumberFormat;
			}

			service.CurrentUserInfo = function () {
				return {
					UserName: userName,
					DefaultTimeZone: defaultTimeZone,
					Language: language,
					DateFormatLocale: dateFormatLocale,
					NumberFormat: numberFormat
				}
			};
		
			service.getCurrentUserFromServer = function () {
				return AuthenticationRequests.getCurrentUser();
			}

			service.initContext = function () {
				var deferred = $q.defer();
				var context = service.getCurrentUserFromServer();

				context.success(function (data) {
					timeout = Date.now() + 10000;
					service.setLocales(data);
					service.SetCurrentUserInfo(data); // remove ?
					service.setBuid();
					deferred.resolve(data);
				});
				return deferred.promise; 
			};

			service.isConnected = function () {
				return timeout > Date.now();
			};

			service.setLocales = function (data) {

				// most calls should be done in a i18n module
				$translate.use(data.Language);
				angularMoment.changeLocale(data.DateFormatLocale);
				// i18nService is for UI Grid localization.
				// Languages supported by it is less than languages in server side (Refer to http://ui-grid.info/docs/#/tutorial/104_i18n).
				// Need do some primary language checking.
				var currentLang = "en";
				var serverSideLang = data.Language.toLowerCase();
				var dashIndex = serverSideLang.indexOf("-");
				var primaryLang = dashIndex > -1 ? serverSideLang.substring(0, dashIndex) : serverSideLang;
				var langs = i18nService.getAllLangs();
				if (langs.indexOf(serverSideLang) > -1) {
					currentLang = serverSideLang;
				} else if (langs.indexOf(primaryLang) > -1) {
					currentLang = primaryLang;
				}
				i18nService.setCurrentLang(currentLang);
			};

			service.setBuid = function () {
				var buid = $sessionStorage.buid;
				if (buid) {
					AuthenticationRequests.setBuidOnHeaders(buid)
				}
			};

			service.resetContext = function () {
				if (window.location.hash) {
					var d = new Date();
					d.setTime(d.getTime() + (5 * 60 * 1000));
					var expires = 'expires=' + d.toUTCString();
					document.cookie = 'returnHash=WFM/' + window.location.hash + '; ' + expires + '; path=/';
				}
				timeout = Date.now();

				$sessionStorage.$reset();
				window.location = 'Authentication';
			};

			return service;
		}
	]);
})();