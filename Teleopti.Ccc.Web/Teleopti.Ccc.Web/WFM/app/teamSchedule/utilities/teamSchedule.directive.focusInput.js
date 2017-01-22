(function() {
	'use strict';

	angular.module('wfm.teamSchedule').directive('focusInput', function($timeout, $interval) {
		return {
			link: function(scope, element, attrs) {
				scope.$watch(attrs.focusInput, function(value) {
					if (value === true) {
						$timeout(function() {
							var focusSearch = $interval(function() {
								angular.element($(element[0]).find('input')).focus();
								$interval.cancel(focusSearch);
							}, 100);
							scope[attrs.focusInput] = false;
						},0);
					}
				});
			}
		};
	});
})();