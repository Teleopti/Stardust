; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigForm1', {
			templateUrl: 'app/gamification/html/measureConfigForm1.tpl.html',
			bindings: {
				disabled: '<readOnly',
				badgeThreshold: '<',
				onUpdate: '&',
				valueFormat: '<',
				max: '<',
				valueDataType: '<',
			},
			controller: ['$scope', function measureConfigForm1Ctrl($scope) {
				var ctrl = this;

				ctrl.updateThreshold = function (invalid) {
					if (invalid) {
						ctrl._badgeThreshold = ctrl.badgeThreshold;
					} else {
						ctrl.onUpdate({ badgeThreshold: ctrl._badgeThreshold });
					}
				};

				ctrl.keyUp = function (event) {
					if (event && event.which === 13) {
						event.target.blur();
					}
				}

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.badgeThreshold) {
						ctrl._badgeThreshold = changesObj.badgeThreshold.currentValue;
					}
				};
			}]
		});
})(angular);