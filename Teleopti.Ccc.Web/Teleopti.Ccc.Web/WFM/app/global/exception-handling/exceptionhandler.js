(function () {
	'use strict';
	angular
		.module('wfm.exceptionHandler', [])
		.config(exceptionHandler);

		function exceptionHandler($provide){
			$provide.decorator("$exceptionHandler", ['$delegate', function ($delegate) {
					return function (error, cause) {
						if (window.onerror) window.onerror(error.message + ' - ' + error.stack);
						$delegate(error, cause);
					};
				}]);
		}
})();
