(function() {
    'use strict';

    angular.module('wfm.outbound').directive('rangeValidate', function () {
        return {
            restrict: 'A',
            require: "ngModel",
            scope: {
                'rangeMin': '@?',
                'rangeMax': '@?',
				'integerOnly': '@?'
            },
            link: postLink
        };
        function postLink(scope, elem, attr, ngModel) {
            ngModel.$validators.range = function (modelValue, viewValue) {
                if (modelValue === null) return true;
                if (angular.isDefined(scope.rangeMin)) {
                    var min = parseInt(scope.rangeMin);
                    if (modelValue < min) return false;
                }

                if (angular.isDefined(scope.rangeMax)) {
                    var max = parseInt(scope.rangeMax);
                    if (modelValue > max) return false;
                }
                return true;
            };

            ngModel.$validators.number = function (modelValue, viewValue) {            
	            if (angular.isDefined(scope.integerOnly) && scope.integerOnly !== 'true') {		         
            		if (!/^\s*[-+]?[0-9]*(\.[0-9]*)?\s*$/.test(viewValue)) return false;					
				} else {
            		if (!/^\s*[-+]?[0-9]*\s*$/.test(viewValue)) return false;
				}										
                var parsedvv = parseInt(viewValue);
                if (isNaN(parsedvv)) return false;
                return true;
            };

            ngModel.$parsers.push(function (viewValue) {
	            if (angular.isDefined(scope.integerOnly) && scope.integerOnly !== 'true') {
		            return parseFloat(viewValue);
	            } else {
	            	return parseInt(viewValue);
	            }	           
            });

            scope.$on("campaign.view.refresh", function () {
                ngModel.$setViewValue(ngModel.$modelValue);
                ngModel.$render();
                ngModel.$setPristine();
            });
        }
    });

})();