;(function (angular) { 'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigHeader', {
			templateUrl: 'app/gamification/html/measureConfigHeader.tpl.html',
			bindings: {
				name: '<',
				allowRename: '<',
				checked: '<',
				onNameUpdate: '&',
				onCheckboxUpdate: '&'
			},
			controller: [function MeasureConfigHeaderCtrl() {
				var ctrl = this;
				ctrl.renameMode = false;
				ctrl.rename = function () {
					ctrl.renameMode = true;
				};
				ctrl.finishRename = function () {
					ctrl.renameMode = false;
				};
				ctrl.onNameChange = function () {
					ctrl.finishRename();
					ctrl.onNameUpdate({ name: ctrl._name });
				};
				ctrl.onCheckboxChange = function () {
					ctrl.onCheckboxUpdate({ enabled: ctrl._checked });
				};

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.name) {
						ctrl._name = changesObj.name.currentValue;
					}
					if (changesObj.checked) {
						ctrl._checked = changesObj.checked.currentValue;
					}
				}
			}]
		});
})(angular);