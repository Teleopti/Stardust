(function() {
	'use strict';

	angular
		.module('wfm.http', ['currentUserInfoService', 'angular-growl', 'pascalprecht.translate'])
		.factory('httpInterceptor', ['$q', 'growl', '$injector', '$timeout', '$translate', httpInterceptor]);

	function httpInterceptor($q, growl, $injector, $timeout, $translate) {
		var connected = true;
		var service = {
			'responseError': reject,
			'request': request
		};
		return service;

		function request(config) {
			if (!connected) {
				var q = $q.defer();
				$timeout(function() {
					q.reject();
				}, 2000);
				return q.promise;
			}
			return config;
		};

		function reject(rejection) {
			if (rejection.status === 401) {
				connected = false;
				var CurrentUserInfo = $injector.get('CurrentUserInfo');
				CurrentUserInfo.resetContext();
			}
			if (rejection.status > 401 && rejection.status < 600 || rejection.status === 0) {
				growl.error("<i class='mdi mdi-alert'></i>" + $translate.instant('InternalErrorMessage'), {
					ttl: 0,
					disableCountDown: true
				});
			}
			return $q.reject(rejection);
		};
	}
})();
