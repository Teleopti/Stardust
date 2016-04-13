(function () {
	'use strict';

	angular.module('currentUserInfoService', ['angularMoment', 'ngStorage', 'wfm.i18n', 'wfm.businessunits', 'wfm.themes'])
		.service('AuthenticationRequests', ['$injector', function ($injector) {
			var service = {};

			service.getCurrentUser = function () {
				var $http = $injector.get('$http');
				return $http.get('../api/Global/User/CurrentUser');
			};

			return service;
		}])
		.service('CurrentUserInfo', ['AuthenticationRequests', '$q', '$sessionStorage', 'wfmI18nService', 'BusinessUnitsService', 'ThemeService',
			function (AuthenticationRequests, $q, $sessionStorage, wfmI18nService, BusinessUnitsService, ThemeService) {
				var userName;
				var defaultTimeZone;
				var language;
				var dateFormatLocale;
				var timeout;
				var theme;
				var service = {};

				service.SetCurrentUserInfo = function (data) {
					userName = data.UserName;
					defaultTimeZone = data.DefaultTimeZone;
					language = data.Language;
					dateFormatLocale = data.DateFormatLocale;
					theme = data.Theme;
				}

				service.CurrentUserInfo = function () {
					return {
						UserName: userName,
						DefaultTimeZone: defaultTimeZone,
						Language: language,
						DateFormatLocale: dateFormatLocale,
						Theme: theme
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
						wfmI18nService.setLocales(data);
						service.SetCurrentUserInfo(data);
						BusinessUnitsService.initBusinessUnit();
						ThemeService.init();
						deferred.resolve(data);
					});
					return deferred.promise;
				};

				service.isConnected = function () {
					return timeout > Date.now();
				};

				service.resetContext = function () {
					if (window.location.hash.length > "#/".length) {
						var d = new Date();
						d.setTime(d.getTime() + (5 * 60 * 1000));
						var expires = 'expires=' + d.toUTCString();
						document.cookie = 'returnHash=WFM/' + window.location.hash + '; ' + expires + '; path=/';
					}
					timeout = Date.now();
					$sessionStorage.$reset();
					window.location.href = 'Authentication';
				};

				return service;
			}]);



})();
