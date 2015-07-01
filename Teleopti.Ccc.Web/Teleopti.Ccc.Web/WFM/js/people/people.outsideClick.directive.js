'use strict';

(function () {

	angular.module('wfm.people')
	.directive('outsideClick', ['$document', '$parse',  function ($window, $parse) {
		return {
			restrict: 'A',
			link: linkFunction
		};

		function linkFunction(scope, element, attrs) {
			var outsideClickHandler = $parse(attrs.outsideClick);
			angular.element($window).on('click', function (event) {
				if (element[0].contains(event.target)) return;
				outsideClickHandler(scope, { $event: event });
				scope.$apply();
			});
		}
	}]);

}());