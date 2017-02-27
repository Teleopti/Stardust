(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('debounceService', factory);

	factory.$inject = ['$q'];

	function factory($q) {

		function debounce(func, wait) {
			var timeout;
			return function () {
				var context = this, args = arguments;
				return $q(function (resolve) {
					var later = function () {
						timeout = null;
						resolve(func.apply(context, args));
					};
					clearTimeout(timeout);
					timeout = setTimeout(later, wait);
				});
			};
		};

		return {
			debounce: debounce
		}
	}
})();
