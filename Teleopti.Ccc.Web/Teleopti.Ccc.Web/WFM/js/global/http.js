(function () {
	'use strict';

	angular
        .module('wfm.http', ['currentUserInfoService'])
        .factory('httpInterceptor', ['$q', 'growl', '$injector', '$timeout', httpInterceptor]);

	function httpInterceptor($q, growl, $injector, $timeout) {
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
			return $q.reject(rejection);
		};
	}
})();