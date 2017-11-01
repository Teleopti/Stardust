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
			var tag = 'md-virtual-repeat-container';
			var rows = $element.find(tag)[0];

			var top = rows.getBoundingClientRect().top || rows.getBoundingClientRect().y;

			var bottomMargin = 18;

			var height = 'calc(100vh - ' + (top + bottomMargin) + 'px)';

			rows.style.height = height;
			$scope.$broadcast('$md-resize');
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

		function Rows(rows) {
			this.rows = rows || [];
			this.numRows = this.rows.length;
		}

		Rows.prototype.getItemAtIndex = function (index) {
			return this.rows[index];
		};

		Rows.prototype.getLength = function () {
			return this.numRows;
		};

		ctrl.title = 'Gamification Targets Table';

		ctrl.teams = teams(1000);

		ctrl.rows = new Rows(ctrl.teams);

		$scope.$on('gamification.selectTargetsTab', function (event, args) {
			setHeightToFillAvailableSpace();
		});
	}

})(angular);
