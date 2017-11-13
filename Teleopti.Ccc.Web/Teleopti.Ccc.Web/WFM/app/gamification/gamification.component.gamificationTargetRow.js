(function (angular) { 'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargetRow', {

			templateUrl: 'app/gamification/html/g.component.gamificationTargetRow.tpl.html',

			require: {
				rootController: '^gamificationTargets'
			},

			bindings: {
				id: '<',
				name: '<',
				appliedSettingId: '<',
				selected: '<',
				onSelect: '&',
				onSettingChange: '&'
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
			}
		}

		ctrl.$onInit = function () {
			ctrl.settingOptions = ctrl.rootController.settings;
		};

		ctrl.isSelected = function () {
			ctrl.selected({ teamId: ctrl.row.teamId });
		};

		ctrl.rowSelected = function () { ctrl.onSelect({ teamId: ctrl.id }); };

	}

})(angular);