;(function (angular) { 'use strict';
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
				updateGold: '&'
			},
			controller: [function measureConfigForm2Ctrl() {
				var ctrl = this;

				ctrl.updateGoldThreshold = function () {
					if (!ctrl.updateGold) return;
					ctrl.updateGold({ goldBadgeThreshold: ctrl.goldBadgeThreshold });
				};

				ctrl.updateSilverThreshold = function () {
					if (!ctrl.updateSilver) return;
					ctrl.updateSilver({ silverBadgeThreshold: ctrl.silverBadgeThreshold });
				};

				ctrl.updateBronzeThreshold = function () {
					if (!ctrl.updateBronze) return;
					ctrl.updateBronze({ bronzeBadgeThreshold: ctrl.bronzeBadgeThreshold });
				}
			}]
		});
})(angular);