(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargetRow', {

			templateUrl: 'app/gamification/html/g.component.gamificationTargetRow.tpl.html',

			require: {
				tableController: '^gamificationTargetsTable'
			},

			bindings: {
				id: '<',
				name: '<',
				appliedSettingValue: '<',
				selected: '<',
				onSelect: '&',
				onSettingChange: '&'
			},

			controller: [GamificationTargetRowController]
		});

	function GamificationTargetRowController() {
		var ctrl = this;

		ctrl.appliedSettingChanged = function () {
			// console.log(ctrl.appliedSettingValue);
			ctrl.onSettingChange({
				teamId: ctrl.id,
				newSettingValue: ctrl.appliedSettingValue
			});
		};

		ctrl.$onInit = function () {
			ctrl.settingOptions = ctrl.tableController.settings;
		};

		ctrl.isSelected = function () {
			ctrl.selected({ teamId: ctrl.row.teamId });
		};

		ctrl.rowSelected = function () { ctrl.onSelect({ teamId: ctrl.id }); };

	}

})(angular);