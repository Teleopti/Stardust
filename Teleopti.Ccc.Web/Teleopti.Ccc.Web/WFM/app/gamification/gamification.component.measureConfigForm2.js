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

					checkValueOrder();
				}

				function checkValueOrder() {
					if (!ctrl.valueDataType || !ctrl.valueOrder) {
						return;
					}
					var valueOrder = ctrl.valueOrder;
					if (valueOrder == 'asc') {
						if (ctrl.valueDataType == '0' || ctrl.valueDataType == '1') {
							var glodValue = parseFloat(ctrl._goldBadgeThreshold);
							if (glodValue > parseFloat(ctrl._silverBadgeThreshold) || glodValue > parseFloat(ctrl._bronzeBadgeThreshold)) {
								ctrl.errorMsg = 'GoldValueShouldSmallerThenRest';
							}
							else {
								ctrl.errorMsg = '';
							}
						}
						else {
							var glodValue = convertValueForTime(ctrl._goldBadgeThreshold);
							if (glodValue > convertValueForTime(ctrl._silverBadgeThreshold) || glodValue > convertValueForTime(ctrl._bronzeBadgeThreshold)) {
								ctrl.errorMsg = 'GoldValueShouldSmallerThenRest';
							} else {
								ctrl.errorMsg = '';
							}
						}
					} else if (valueOrder == 'desc') {
						if (ctrl.valueDataType == '0' || ctrl.valueDataType == '1') {
							var glodValue = parseFloat(ctrl._goldBadgeThreshold);
							if (glodValue < parseFloat(ctrl._silverBadgeThreshold) || glodValue < parseFloat(ctrl._bronzeBadgeThreshold)) {
								ctrl.errorMsg = "GoldValueShouldLargerThenRest";
							}
							else {
								ctrl.errorMsg = '';
							}
						}
						else {
							var glodValue = convertValueForTime(ctrl._goldBadgeThreshold);
							if (glodValue < convertValueForTime(ctrl._silverBadgeThreshold) || glodValue < convertValueForTime(ctrl._bronzeBadgeThreshold)) {
								ctrl.errorMsg = 'GoldValueShouldLargerThenRest';
							} else {
								ctrl.errorMsg = '';
							}
						}
					}
				}

				function convertValueForTime(value) {
					var subValues = value.split(':');
					var hourValue = convertTimeToNumber(subValues[0], 100);
					var minuteValue = convertTimeToNumber(subValues[1], 10);
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
			}]
		});
})(angular);