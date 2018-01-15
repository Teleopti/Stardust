;(function (angular) { 'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigForm1', {
			templateUrl: 'app/gamification/html/measureConfigForm1.tpl.html',
			bindings: {
				badgeThreshold: '<',
				onUpdateConfig:'<',
				dataType:'<'
			},
			controller: [function measureConfigForm1Ctrl() {
				var ctrl = this;
				ctrl.hasError = false;
				ctrl.updateThreshould = function () {
					if (!ctrl.hasError) {
						ctrl.onUpdateConfig(ctrl._badgeThreshold);
					}else{
						ctrl._badgeThreshold = ctrl.badgeThreshold;
						ctrl.hasError = false;
						ctrl.errorMsg = '';
					}
				}

				ctrl.validate = function () {
					var pattern = /^[1-9]\d*.\d*|0.\d*[1-9]\d*$/;
					if (ctrl.dataType == 1) {
						pattern = /^0.\d+$/;
					}

					if (!pattern.test(ctrl._badgeThreshold)) {
						ctrl.hasError = true;
						ctrl.errorMsg = 'invalidFormat';
					}else{
						ctrl.hasError = false;
						ctrl.errorMsg = '';
					}
				}

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.badgeThreshold) {
						ctrl._badgeThreshold = changesObj.badgeThreshold.currentValue;
					}

				}


			}]
		});
})(angular);