; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigHeader', {
			templateUrl: 'app/gamification/html/measureConfigHeader.tpl.html',
			bindings: {
				name: '<',
				allowRename: '<',
				checked: '<',
				onNameUpdate: '&',
				onEnable: '&',
				valueDataType: '<'
			},
			controller: [function MeasureConfigHeaderCtrl() {
				var ctrl = this;
				ctrl.renameMode = false;
				ctrl.rename = function () {
					ctrl.renameMode = true;
				};
				ctrl.finishRename = function () {
					if (!ctrl._name || !ctrl._name.length) ctrl._name = ctrl.name;
					ctrl.renameMode = false;
				};
				ctrl.onNameChange = function () {
					ctrl.finishRename();
					if (ctrl.onNameUpdate && ctrl._name !== ctrl.name) ctrl.onNameUpdate({ name: ctrl._name });
				};
				ctrl.onCheckboxClick = function () {
					ctrl.onEnable({ enable: !ctrl.checked });
				};

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.name) {
						ctrl._name = changesObj.name.currentValue;
					}
				};

				ctrl.preventExpanding = function (configIsEnabled, event) {
					if (!configIsEnabled) event.stopPropagation();
				};
			}]
		});
})(angular);