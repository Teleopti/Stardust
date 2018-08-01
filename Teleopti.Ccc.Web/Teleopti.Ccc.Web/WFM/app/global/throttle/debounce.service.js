(function () {
	'use strict';

	angular.module('wfm.throttle').factory('throttleDebounce', ["$timeout", function ($timeout) {
		return function (callback, delay) {
			delay = delay || 0;

			var deferTimer;
			return function () {
				var args = arguments,
					context = this;
				$timeout.cancel(deferTimer);
				deferTimer = $timeout(function () {
					return callback.apply(context, args);
				}, delay);
				return deferTimer;
			};
		};
	}]);

})();