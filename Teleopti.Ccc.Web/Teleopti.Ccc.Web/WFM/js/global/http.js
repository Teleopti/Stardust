(function () {
	'use strict';

	angular
        .module('wfm.http', [])
        .factory('httpInterceptor', ['$q','growl', '$sessionStorage', httpInterceptor]);

	function httpInterceptor($q, growl, $sessionStorage) {
		/*var service = {
			'responseError': reject
		};
		return service;

		function reject(rejection) {
			if (rejection.status === 500 || rejection.status === 401) {
				growl.error("<i class='mdi mdi-alert-octagon'></i> An error occured while contacting the server. This page will be refreshed.", {
					ttl: 5000,
					disableCountDown: true,
					onclose: (function() {
						if (window.location.hash) {
							var d = new Date();
							d.setTime(d.getTime() + (5 * 60 * 1000));
							var expires = 'expires=' + d.toUTCString();
							document.cookie = 'returnHash=WFM' + window.location.hash + '; ' + expires + '; path=/';
						}

						$sessionStorage.$reset();
						window.location = 'Authentication';
					})
				});
				return $q.reject(rejection);
			} else {
				return rejection;
			}
		};*/
		return {};
	}
})();