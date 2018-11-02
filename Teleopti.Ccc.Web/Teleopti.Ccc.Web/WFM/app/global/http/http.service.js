(function() {
	'use strict';

	angular
		.module('wfm.http', ['currentUserInfoService', 'wfm.notice', 'pascalprecht.translate', 'wfm.versionService'])
		.factory('httpInterceptor', httpInterceptor);

	httpInterceptor.$inject = ['$q', '$injector', '$timeout', '$translate', '$window', 'versionService'];

	function httpInterceptor($q, $injector, $timeout, $translate, $window, versionService) {
		var connected = true;

		var service = {
			responseError: onResponseError,
			request: onRequest,
			response: onResponse
		};
		return service;

		function ensureClientIsUpToDate(headers) {
			var version = versionService.getVersion();
			var newVersion = "";
			if (headers)
				newVersion = headers['X-Server-Version'] || '';
			if (newVersion.length === 0) return;
			else if (version.length === 0) versionService.setVersion(newVersion);
			else if (version !== newVersion) $window.location.reload(true);
		}

		function onRequest(config) {
			if (!connected) {
				var q = $q.defer();
				$timeout(function() {
					q.reject();
				}, 2000);
				return q.promise;
			}

			// Register the current business unit as a header
			var businessUnitId = sessionStorage.getItem('buid');
			if (businessUnitId) config.headers['X-Business-Unit-Filter'] = businessUnitId;

			return config;
		}

		function onResponse(response) {
			ensureClientIsUpToDate(response.headers);
			return response;
		}

		//This is bad and should be reworked. //Anders Sjöberg 2018-07-31
		function onResponseError(rejection) {
			var NoticeService = $injector.get('NoticeService');
			var Settings = $injector.get('Settings');

			ensureClientIsUpToDate(rejection.headers);

			switch (true) {
				case rejection.config &&
					rejection.config.timeout &&
					rejection.config.timeout.$$state.value === 'cancel':
					break;

				//Super special case because this is bad practice, so we add even more bad stuff
				case rejection.config &&
					rejection.config.url &&
					rejection.config.url.indexOf('exportskilldatatoexcel') >= 0:
					break;

				case rejection.status === -1:
					//don't remove class test-alert - used in perf tests
					NoticeService.error(
						"<span class='test-alert'></span>" + $translate.instant('ConnectionErrorMessage'),
						null,
						false
					);
					break;

				case rejection.status === 400:
					NoticeService.error(
						"<span class='test-alert'></span>" + $translate.instant('BadRequest'),
						null,
						false
					);
					break;

				case rejection.status === 401:
					connected = false;
					var CurrentUserInfo = $injector.get('CurrentUserInfo');
					CurrentUserInfo.resetContext();
					NoticeService.error(
						"<span class='test-alert'></span>" + $translate.instant('NoPermissionToViewErrorMessage'),
						null,
						false
					);
					break;

				case rejection.status === 403:
					NoticeService.error(
						"<span class='test-alert'></span>" + $translate.instant('NoPermissionToViewErrorMessage'),
						null,
						false
					);
					break;

				case rejection.status === 422:
					/* 422 Unprocessable Entity */
					break;

				case rejection.status === 409:
					NoticeService.error(
						"<span class='test-alert'></span>" + $translate.instant('OptimisticLockText'),
						null,
						false
					);
					break;

				case rejection.status > 403 && rejection.status < 600:
					//don't remove class test-alert - used in perf tests
					NoticeService.error(
						"<span class='test-alert'></span>" +
							$translate.instant('InternalErrorMessage') +
							'<a href="mailto:' +
							Settings.supportEmailSetting +
							'">' +
							Settings.supportEmailSetting +
							'</a>',
						null,
						false
					);
					break;
			}
			return $q.reject(rejection);
		}
	}
})();
