(function() {
	'use strict';

	angular.module('wfm.focusInput', []).directive('focusInput', function ($timeout, $interval) {
		return {
			link: function(scope, element, attrs) {
				scope.$watch(attrs.focusInput, function(value) {
					if (value) {
						$timeout(function() {
							var focusInputInterval = $interval(function() {
								angular.element($(element[0]).find('input')).focus();
								$interval.cancel(focusInputInterval);
							}, 100);
							//scope[attrs.focusInput] = true;
						},0);
					}else{
						$timeout(function() {
							var blurInputInterval = $interval(function() {
								angular.element($(element[0]).find('input')).blur();
								$interval.cancel(blurInputInterval);
							}, 100);
							//scope[attrs.focusInput] = false;
						},0);
					}
				});
			}
		};
	});
})();