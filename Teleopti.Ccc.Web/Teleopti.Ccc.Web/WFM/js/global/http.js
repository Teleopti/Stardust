(function () {
	'use strict';

	angular
        .module('wfm.http', ['currentUserInfoService'])
        .factory('httpInterceptor', ['$q', 'growl', '$injector', httpInterceptor]);

	function httpInterceptor($q, growl, $injector) {
		var service = {
			'responseError': reject
		};
		return service;

		function reject(rejection) {
			if (rejection.status === 401) {
				var CurrentUserInfo = $injector.get('CurrentUserInfo');
				CurrentUserInfo.resetContext();
			} 
			return $q.reject(rejection);
		};
	}
})();