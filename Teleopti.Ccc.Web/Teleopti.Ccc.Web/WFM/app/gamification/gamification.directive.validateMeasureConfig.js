;(function (angular) { 'use strict';
	angular.module('wfm.gamification')
	.directive('thresholdValidator', ['$parse', function ($parse) {
		return {
			restrict: 'A',
			require: ['ngModel', '^?measureConfigForm1', '^?measureConfigForm2'],
			link: function (scope, element, attrs, ctrls) {
				var ngModel = ctrls[0];
				var measureConfigForm = ctrls[1] || ctrls[2];

				if (!ngModel || !measureConfigForm) return;

				var pattern = new RegExp(attrs.thresholdValidator);

				ngModel.$validators.validFormat = function (modelValue, viewValue) {
					var valid = validate(modelValue, pattern);
					if (valid)
						scope.$evalAsync(function () {
							measureConfigForm.updateThreshold();
						});
					return valid;
				};

				function validate(value, pattern) {
					return pattern.test(value);
				}
			}
		};
	}]);
})(angular);