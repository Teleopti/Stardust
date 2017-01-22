(function() {
	'use strict';

	angular.module('wfm.teamSchedule').directive('focusInput', function($rootScope) {
		return {
			link: function(scope, element, attrs) {
				scope.$watch(attrs.focusInput, function(value) {
					if (value === true) {
						var focusSearch = window.setInterval(function() {
							if ($rootScope.$$phase != 'digest') {
								angular.element($(element[0]).find('input')).focus();
								window.clearInterval(focusSearch);
							}
						}, 100);
						scope[attrs.focusInput] = false;
					}
				});
			}
		};
	});
})();