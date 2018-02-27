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
				valueOrder: '<'
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
				};

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
				};

				ctrl.keyUp = function (event) {
					if (event && event.which === 13) {
						event.target.blur();
					}

					checkValueOrder();
				};

				ctrl.$onInit = function () {
					checkValueOrder();
				};

				function checkValueOrder() {
					if (angular.isUndefined(ctrl.valueDataType) || !ctrl.valueOrder) return;

					var gold, silver, bronze;

					ctrl.errorMsg = '';

					switch (ctrl.valueDataType) {
						case 2:
							gold = convertValueForTime(ctrl._goldBadgeThreshold);
							silver = convertValueForTime(ctrl._silverBadgeThreshold);
							bronze = convertValueForTime(ctrl._bronzeBadgeThreshold);
							break;

						default:
							gold = parseFloat(ctrl._goldBadgeThreshold);
							silver = parseFloat(ctrl._silverBadgeThreshold);
							bronze = parseFloat(ctrl._bronzeBadgeThreshold);
							break;
					}

					switch (ctrl.valueOrder) {
						case 'asc':
							if (!valuesAreInAscendingOrder(gold, silver, bronze))
								ctrl.errorMsg = 'ValuesShouldBeInAscendingOrderFromGoldToBronze';
							break;

						case 'desc':
							if (!valuesAreInDescendingOrder(gold, silver, bronze))
								ctrl.errorMsg = 'ValuesShouldBeInDescendingOrderFromGoldToBronze';
							break;
					}



					function valuesAreInAscendingOrder(gold, silver, bronze) {
						return gold < silver && gold < bronze && silver < bronze;
					}

					function valuesAreInDescendingOrder(gold, silver, bronze) {
						return gold > silver && gold > bronze && silver > bronze;
					}

					function convertValueForTime(value) {
						var subValues = value.split(':');
						var hourValue = convertTimeToNumber(subValues[0], 10000);
						var minuteValue = convertTimeToNumber(subValues[1], 100);
						var secondValue = convertTimeToNumber(subValues[2], 1);
						return hourValue + minuteValue + secondValue;
					}

					function convertTimeToNumber(time, scale) {
						var result = 0;

						if (time == '00') {
							result = 0;
						} else {
							if (time.indexOf('0') == 0) {
								result = parseInt(time.substr(1)) * scale
							} else {
								result = parseInt(time) * scale;
							}
						}

						return result;
					}
				}

			}]
		});
})(angular);