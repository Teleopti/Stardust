(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamification', {
			templateUrl: 'app/gamification/html/g.component.gamification.tpl.html',
			controller: ['$element', '$scope', GamificationController]
		});

	function GamificationController($element, $scope) {

		var ctrl = this;

		var element = $element[0];

		ctrl.setElementHeightToCoverRestOfViewport = function () {

			var top = element.getBoundingClientRect().top || element.getBoundingClientRect().y;

			var bottomMargin = 18;

			var height = 'calc(100vh - ' + (top + bottomMargin) + 'px)';

			element.style.height = height;

			$scope.$broadcast('gamification.selectTargetsTab');

		};

		ctrl.resetElementHeight = function () { element.style.height = ''; };

	}

})(angular);