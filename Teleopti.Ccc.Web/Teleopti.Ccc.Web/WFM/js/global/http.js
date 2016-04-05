(function() {
	'use strict';

	angular
		.module('wfm.http', ['currentUserInfoService', 'wfm.notice', 'pascalprecht.translate'])
		.factory('httpInterceptor', [
			'$q', 'NoticeService', '$injector', '$timeout', '$translate', function($q, NoticeService, $injector, $timeout, $translate) {
				var connected = true;

				function request(config) {
					if (!connected) {
						var q = $q.defer();
						$timeout(function () {
							q.reject();
						}, 2000);
						return q.promise;
					}
					return config;
				}

				function reject(rejection) {
					switch (true) {
						case (rejection.status === 0):
							//don't remove class test-alert - used in perf tests
							NoticeService.error("<span class='test-alert'></span>" + $translate.instant('ConnectionErrorMessage'), null, false);
							break;

						case (rejection.status === 401):
							connected = false;
							var CurrentUserInfo = $injector.get('CurrentUserInfo');
							CurrentUserInfo.resetContext();
							break;

						case (rejection.status > 401 && rejection.status < 600):
							//don't remove class test-alert - used in perf tests
							NoticeService.error("<span class='test-alert'></span>" + $translate.instant('InternalErrorMessage'), null, false);
							break;
					}

					return $q.reject(rejection);
				}

				var service = {
					'responseError': reject,
					'request': request
				};
				return service;
			}
		]);
})();
