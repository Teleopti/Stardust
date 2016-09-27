(function() {
	'use strict';

	angular
		.module('wfm.http', ['currentUserInfoService', 'wfm.notice', 'pascalprecht.translate'])
		.factory('httpInterceptor', [
			'$q', '$injector', '$timeout', '$translate', function($q, $injector, $timeout, $translate) {
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
					var NoticeService = $injector.get('NoticeService');
					var Settings = $injector.get('Settings');
					switch (true) {
						case (rejection.status === -1):
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
							NoticeService.error("<span class='test-alert'></span>" + $translate.instant('InternalErrorMessage') +
								'<a href="mailto:' + Settings.supportEmailSetting + '">' + Settings.supportEmailSetting + '</a>', null, false);
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
