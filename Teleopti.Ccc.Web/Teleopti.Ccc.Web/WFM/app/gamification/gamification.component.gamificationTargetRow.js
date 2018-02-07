(function (angular) { 'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargetRow', {

			templateUrl: 'app/gamification/html/g.component.gamificationTargetRow.tpl.html',

			bindings: {
				appliedSettingId: '<',
				id: '<',
				name: '<',
				onSelect: '&',
				onSettingChange: '&',
				selected: '<',
				settingOptions: '<',
			},

			controller: [
				'$element',
				GamificationTargetRowController
			]
		});

	function GamificationTargetRowController($element) {
		var ctrl = this;

		ctrl.EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

		ctrl.appliedSettingChanged = function () {
			ctrl.onSettingChange({
				teamId: ctrl.id,
				newSettingValue: ctrl.appliedSettingId
			});
		};

		ctrl.$onChanges = function (changesObj) {
			if (changesObj.selected) {
				$element.attr('is-selected', changesObj.selected.currentValue ? 'true' : 'false');

				if (changesObj.selected.currentValue)
					$element.find('md-select').attr('tabindex', '0');
				else
					$element.find('md-select').attr('tabindex', '-1');
			}
		}

		ctrl.$onInit = function () {
		};

		ctrl.isSelected = function () {
			ctrl.selected({ teamId: ctrl.row.teamId });
		};

		ctrl.rowSelected = function () { ctrl.onSelect({ teamId: ctrl.id }); };

	}

})(angular);