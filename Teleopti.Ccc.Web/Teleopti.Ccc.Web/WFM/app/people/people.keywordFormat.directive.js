'use strict';

(function () {

	angular.module('wfm.people')
	.directive('keywordFormat', ['$translate', function ($translate) {
		return {
			restrict: 'A',
			require: '?ngModel',
			link: linkFunction
		};

		function linkFunction(scope, element, attrs, ctrl) {
			if (!ctrl) return;

			ctrl.$formatters.unshift(function () {
				var searchExpressionSeprator = ";";
				var keyValueSeprator = ":";
				var modelValue = ctrl.$modelValue;
				var formattedValues = [];
				var expressions = modelValue.split(searchExpressionSeprator);
				angular.forEach(expressions, function (expression) {
					var items = expression.split(keyValueSeprator);
					var key = items[0];
					var value = items[1];
					if (value) {
						var displayKey = $translate.instant(key);
						formattedValues.push(displayKey + keyValueSeprator + value);
					}
				});
				if (formattedValues.length > 0) {
					return formattedValues.join(searchExpressionSeprator);
				}
				return modelValue;
			});
		}
	}]);

}());