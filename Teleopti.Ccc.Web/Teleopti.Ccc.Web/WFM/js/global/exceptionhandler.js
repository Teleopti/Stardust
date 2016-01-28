(function () {
	'use strict';

	angular
		.module('wfm.exceptionHandler', [])
		.factory('$exceptionHandler',function () {
				var service = {
					'errorCatcherHandler': errorCatcherHandler
				};
				return service;

				function errorCatcherHandler(exception, cause) {
					console.error(exception);
					if (window.OnClientError) window.OnClientError(exception);
				};
			}
		);
})();