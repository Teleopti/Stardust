(function() {
	'use strict';

	angular.module('wfm.teamSchedule').directive('waitFor', waitForDirective);

	waitForDirective.$inject = ['$animate', 'bootstrapCommon'];

	function waitForDirective($animate, bootstrapCommon) {
		return {
			transclude: 'element',
			priority: 1500,
			terminal: true,
			restrict: 'A',
			link: function($scope, $element, $attr, ctrl, $transclude) {								
				
				bootstrapCommon.onReady().then(function() {
					return $scope.$eval($attr.waitFor);
				}).then(function() {
					$transclude(function(clone) {
						$animate.enter(clone, $element.parent(), $element);
					});
				});
			}
		};
	}

})();