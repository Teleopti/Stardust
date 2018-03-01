(function(angular) {
	'use strict';

	angular.module('wfm.gamification').component('gamification', {
		templateUrl: 'app/gamification/html/g.component.gamification.tpl.html',
		controller: ['$element', '$scope', '$state', 'Toggle', GamificationController]
	});

	function GamificationController($element, $scope, $state, toggleService) {
		var ctrl = this;

		var element = $element[0];

		ctrl.toggle = {};

		ctrl.onSelectTab = function(tab) {
			$state.go('gamification.' + tab);
		};

		ctrl.$onInit = function() {
			toggleService.togglesLoaded.then(function() {
				var recalBadges = 'WFM_Gamification_Recalculate_Badges_Within_Period_48403';
				ctrl.toggle[recalBadges] = toggleService[recalBadges];
			});

			openTab($state.current.name);

			function openTab(state) {
				ctrl.selectedTab = 0;
				if (state.indexOf('targets') > -1) {
					ctrl.selectedTab = 1;
				} else if (state.indexOf('import') > -1) {
					ctrl.selectedTab = 2;
				} else if (state.indexOf('calculation') > -1) {
					ctrl.selectedTab = 3;
				}
			}
		};

		ctrl.setElementHeightToCoverRestOfViewport = function() {
			var top = element.getBoundingClientRect().top || element.getBoundingClientRect().y;

			var bottomMargin = 18;

			var height = 'calc(100vh - ' + (top + bottomMargin) + 'px)';

			element.style.height = height;

			$scope.$broadcast('gamification.selectTargetsTab');
		};

		ctrl.resetElementHeight = function() {
			element.style.height = '';
		};
	}
})(angular);
