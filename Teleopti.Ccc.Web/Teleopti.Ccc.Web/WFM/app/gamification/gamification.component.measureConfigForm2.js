; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.component('measureConfigForm2', {
			templateUrl: 'app/gamification/html/measureConfigForm2.tpl.html',
			bindings: {
				disabled: '<readOnly',
				bronzeBadgeThreshold: '<',
				updateBronze: '&',
				silverBadgeThreshold: '<',
				updateSilver: '&',
				goldBadgeThreshold: '<',
				updateGold: '&',
				valueFormat: '<',
				max: '<',
				valueDataType: '<',
			},
			controller: [function measureConfigForm2Ctrl() {
				var ctrl = this;

				ctrl.updateGoldThreshold = function (invalid) {
					if (invalid) {
						ctrl._goldBadgeThreshold = ctrl.goldBadgeThreshold
					} else {
						if (!ctrl.updateGold) return;
						ctrl.updateGold({ goldBadgeThreshold: ctrl._goldBadgeThreshold });
					}
				};

				ctrl.updateSilverThreshold = function (invalid) {
					if (invalid) {
						ctrl._silverBadgeThreshold = ctrl.silverBadgeThreshold;
					} else {
						if (!ctrl.updateSilver) return;
						ctrl.updateSilver({ silverBadgeThreshold: ctrl._silverBadgeThreshold });
					}

				};

				ctrl.updateBronzeThreshold = function (invalid) {
					if (invalid) {
						ctrl._bronzeBadgeThreshold = ctrl.bronzeBadgeThreshold;
					}
					else {

						if (!ctrl.updateBronze) return;
						ctrl.updateBronze({ bronzeBadgeThreshold: ctrl._bronzeBadgeThreshold });
					}
				}

				ctrl.$onChanges = function (changesObj) {
					if (changesObj.goldBadgeThreshold) {
						ctrl._goldBadgeThreshold = changesObj.goldBadgeThreshold.currentValue;
					}

					if (changesObj.silverBadgeThreshold) {
						ctrl._silverBadgeThreshold = changesObj.silverBadgeThreshold.currentValue;
					}

					if (changesObj.bronzeBadgeThreshold) {
						ctrl._bronzeBadgeThreshold = changesObj.bronzeBadgeThreshold.currentValue;
					}
				}

				ctrl.keyUp = function (event) {
					if (event && event.which === 13) {
						event.target.blur();
					}
				}
			}]
		});
})(angular);