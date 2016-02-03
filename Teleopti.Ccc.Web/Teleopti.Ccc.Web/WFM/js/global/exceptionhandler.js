(function () {
	'use strict';
	angular
		.module('wfm.exceptionHandler', [])
		.config(['$provide', function ($provide) {
			$provide.decorator("$exceptionHandler", ['$delegate', function ($delegate) {
				return function (error, cause) {
					if (window.onerror) window.onerror(error.message + ' - ' + error.stack);
					$delegate(error, cause);
				};
			}]);
		}]);
})();