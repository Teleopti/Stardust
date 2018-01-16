; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigForm1', {
			templateUrl: 'app/gamification/html/measureConfigForm1.tpl.html',
			bindings: {
				badgeThreshold: '<',
				onUpdate: '&',
				valueFormat: '<',
				max: '<'
			},
			controller: ['$scope', function measureConfigForm1Ctrl($scope) {
				var ctrl = this;

				ctrl.updateThreshold = function () {
					ctrl.onUpdate({ badgeThreshold: ctrl._badgeThreshold });
				};

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.badgeThreshold) {
						ctrl._badgeThreshold = changesObj.badgeThreshold.currentValue;
					}
				};

			}]
		});
})(angular);