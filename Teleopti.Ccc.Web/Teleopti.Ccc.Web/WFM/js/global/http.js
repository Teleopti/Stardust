(function () {
	'use strict';

	angular
        .module('wfm.http', ['currentUserInfoService'])
        .factory('httpInterceptor', ['$q', 'growl', 'CurrentUserInfo', httpInterceptor]);

	function httpInterceptor($q, growl, CurrentUserInfo) {
		var service = {
			'responseError': reject
		};
		return service;

		function reject(rejection) {
			if (rejection.status === 500 || rejection.status === 401) {
				growl.error("<i class='mdi mdi-alert-octagon'></i> An error occured while contacting the server. This page will be refreshed.", {
					ttl: 5000,
					disableCountDown: true,
					onclose: CurrentUserInfo.resetContext
				});
				return $q.reject(rejection);
			} else {
				return rejection;
			}
		};
	}
})();