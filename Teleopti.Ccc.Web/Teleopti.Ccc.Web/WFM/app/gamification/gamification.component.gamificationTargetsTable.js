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

		ctrl.selectedTeamIds = {};

		ctrl.settings = [
			{ value: 'default', name: 'Default' },
			{ value: 'setting1', name: 'Setting 1' },
			{ value: 'setting2', name: 'Setting 2' }
		];

		ctrl.isIndeterminate = function () {
			var numSelected = Object.keys(ctrl.selectedTeamIds)
			.filter(function (id) { return ctrl.selectedTeamIds[id] }).length;
			return numSelected !== 0 && numSelected !== ctrl.teams.length;
		};

		ctrl.allRowsSelected = function () {
			var numSelected = Object.keys(ctrl.selectedTeamIds)
				.filter(function (id) { return ctrl.selectedTeamIds[id] }).length;
			return numSelected === ctrl.teams.length;
		};

		ctrl.toggleAllRows = function () {
			var allSelected = ctrl.allRowsSelected();
			ctrl.teams.forEach(function (team) {
				ctrl.selectedTeamIds[team.teamId] = !allSelected;
			});
		};

		ctrl.selectRow = function (teamId) {
			// console.log('select row: ' + teamId)
			ctrl.selectedTeamIds[teamId] = !ctrl.selectedTeamIds[teamId];
		};

		ctrl.changeAppliedSetting = function (teamId, newSettingValue) {
			// console.log(teamId, newSettingValue)
			Object.keys(ctrl.selectedTeamIds)
				.filter(function (id) { return ctrl.selectedTeamIds[id]; })
				.forEach(function (id) {
					ctrl.teams[id].appliedSettingValue = newSettingValue;
				});
		};

		function refresh() { $scope.$broadcast('$md-resize'); }

		function setHeightToFillAvailableSpace() {
			var tag = 'md-virtual-repeat-container';
			var rows = $element.find(tag)[0];

			var top = rows.getBoundingClientRect().top || rows.getBoundingClientRect().y;

			var bottomMargin = 18;

			var height = 'calc(100vh - ' + (top + bottomMargin) + 'px)';

			rows.style.height = height;

			refresh();
		}

		function teams(n) {
			var teams = [];
			for (var i = 0; i < n; i++) {
				teams.push({
					teamId: i,
					teamName: 'Team ' + (i + 1),
					// appliedSettingValue: ctrl.settings[i % ctrl.settings.length].value
					appliedSettingValue: i === 0 ? ctrl.settings[0].value : ctrl.settings[1].value
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

		$scope.$on('gamification.selectTargetsTab', function (event, args) {
			setHeightToFillAvailableSpace();
			ctrl.teams = teams(1000);
			ctrl.rows = new Rows(ctrl.teams);
		});
	}

})(angular);
