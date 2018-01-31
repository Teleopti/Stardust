; (function (angular) {
	'use strict';
	angular.module('wfm.gamification')
		.directive('thresholdValidator', ['$parse', function ($parse) {
			return {
				restrict: 'A',
				require: ['ngModel', '^?measureConfigForm1', '^?measureConfigForm2'],
				link: function (scope, element, attrs, ctrls) {
					var ngModel = ctrls[0];
					var measureConfigForm = ctrls[1] || ctrls[2];

					if (!ngModel || !measureConfigForm) return;

					var validator = attrs.thresholdValidator;
					if (validator) {
						var validators = validator.split(';')
						var pattern = validators[0];
						var max;
						var dataType;
						if (validators.length === 3) {
							max = validators[1];
							dataType = validators[2];
						}

						ngModel.$validators.validFormat = function (modelValue, viewValue) {
							var value = modelValue || viewValue;
							var valid = validate(value, pattern);

							return valid;
						};
					}


					function validate(value, pattern) {
						var result = new RegExp(pattern).test(value);
						if (max && dataType) {
							switch (dataType) {
								case '0':
								case '1':
									result = result && (parseFloat(value) <= parseFloat(max));
									break;
								default:
									result = result && (convertValueForTime(value) <= convertValueForTime(max));
									break;
							}
						}

						return result;
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
			};
		}]);
})(angular);