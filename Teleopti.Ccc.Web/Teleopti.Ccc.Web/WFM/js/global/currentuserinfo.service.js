(function() {
	'use strict';

	angular.module('currentUserInfoService', ['angularMoment', 'ngStorage', 'wfm.i18n'])
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
		.service('CurrentUserInfo', ['AuthenticationRequests', '$q', '$sessionStorage', 'wfmI18nService',
			function (AuthenticationRequests, $q, $sessionStorage, wfmI18nService) {
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
					wfmI18nService.setLocales(data);
					service.SetCurrentUserInfo(data); // remove ?
					service.setBuid();
					deferred.resolve(data);
				});
				return deferred.promise; 
			};

			service.isConnected = function () {
				return timeout > Date.now();
			};

			service.setBuid = function () {
				var buid = $sessionStorage.buid;
				if (buid) {
					AuthenticationRequests.setBuidOnHeaders(buid);
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