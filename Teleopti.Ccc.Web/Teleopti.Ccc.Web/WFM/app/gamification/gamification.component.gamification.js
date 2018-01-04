(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamification', {
			templateUrl: 'app/gamification/html/g.component.gamification.tpl.html',
			controller: ['$element', '$scope', '$state', GamificationController]
		});

	function GamificationController($element, $scope, $state) {

		var ctrl = this;

		var element = $element[0];

		ctrl.tabSelected = function (tab) {
			$state.go('gamification.' + tab);
		}

		ctrl.$onInit = function () {
			var currentState = $state.current.name;
			if (currentState.indexOf('targets') > -1) {
				ctrl.selectedTab = 1;
			} else
				if (currentState.indexOf('import') > -1) {
					ctrl.selectedTab = 2;
				}
				else {
					ctrl.selectedTab = 0;
				}
		}

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