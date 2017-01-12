(function() {
	'use strict';

	angular.module('wfm.teamSchedule').directive('waitFor', waitForDirective);

	waitForDirective.$inject = ['$animate'];

	function waitForDirective($animate) {
		return {
			transclude: 'element',
			priority: 1500,
			terminal: true,
			restrict: 'A',
			link: function($scope, $element, $attr, ctrl, $transclude) {
				var promise = $scope.$eval($attr.waitFor);
				if (promise) {
					promise.then(function() {
						$transclude(function(clone) {
							$animate.enter(clone, $element.parent(), $element);
						});
					});
				}				
			}
		};
	}

})();