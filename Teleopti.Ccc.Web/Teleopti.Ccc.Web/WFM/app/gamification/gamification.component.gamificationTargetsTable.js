(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargetsTable', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargetsTable.tpl.html',
			controller: ['$element', '$scope', GamificationTargetsTableController]
		});

	function GamificationTargetsTableController($element, $scope) {

		var element = $element[0];

		var ctrl = this;

		function setHeightToFillAvailableSpace() {
			var top = element.getBoundingClientRect().top || element.getBoundingClientRect().y;

			var bottomMargin = 23;

			var height = 'calc(100vh - ' + (top + bottomMargin) + 'px)';

			element.style.height = height;
		}

		function teams(n) {
			var teams = [];
			for (var i = 0; i < n; i++) {
				teams.push({
					id: i,
					name: 'Team ' + (i + 1),
					appliedSetting: 'Default'
				});
			}
			return teams;
		}

		ctrl.title = 'Gamification Targets Table';

		ctrl.teams = teams(5);

		$scope.$on('gamification.selectTargetsTab', function (event, args) {
			setHeightToFillAvailableSpace();
		});
	}

})(angular);
