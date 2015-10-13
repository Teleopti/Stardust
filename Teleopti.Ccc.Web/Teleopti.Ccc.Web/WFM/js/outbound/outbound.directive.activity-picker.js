(function() {
    'use strict';

    angular.module('wfm.outbound').directive('activityPicker', [
		'outboundActivityService',
		function (outboundActivityService) {
		    return {
		        restrict: 'E',
		        require: ['ngModel'],
		        scope: {
		        },
		        templateUrl: 'js/outbound/html/activity-picker.tpl.html',
		        link: postLink
		    };

		    function postLink(scope, elem, attrs, ctrls) {
		        var ngModel = ctrls[0];

		        scope.disableInput = false;
		        scope.disableCreate = false;
		        scope.inputPlaceholder = 'NewActivityName';

		        scope.inputs = { Id: null, Name: '', useExisting: false };
		        scope.allActivities = [];

		        outboundActivityService.listActivity().then(function (data) {
		            scope.allActivities = data;
		            ngModel.$parsers.push(parser);
		            ngModel.$render();
		        });

		        scope.$watch(function () {
		            return scope.inputs;
		        }, function (newValue, oldValue) {

		            ngModel.$setViewValue(angular.copy(scope.inputs));
		            ngModel.$render();
		            if (newValue == oldValue) {
		                ngModel.$setPristine();
		            }

		        }, true);

		        if (angular.isDefined(attrs.syncName)) {
		            attrs.$observe('syncName', function (newValue) {
		                scope.inputs.Name = newValue;
		            });
		            scope.disableInput = true;
		        } else if (angular.isDefined(attrs.disableCreate)) {
		            scope.disableCreate = true;
		        }


		        ngModel.$formatters.push(formatter);

		        ngModel.$validators.notEmpty = function (modelValue, viewValue) {
		            return (viewValue.useExisting) ?
						viewValue.Id !== null :
						viewValue.Name != null && viewValue.Name != '';
		        }


		        // It is not decided whether we should forbide use to create duplicate activities yet.

		        //ngModel.$validators.alreadyExists = function(modelValue, viewValue) {
		        //	if (viewValue.useExisting) return true;
		        //	return scope.allActivities.filter(attrValueFilter('Name', viewValue.Name)).length == 0;
		        //}

		        ngModel.$render = renderer;

		        function renderer() {
		            scope.inputs = ngModel.$viewValue || { Id: null, Name: '', useExisting: false };
		        }

		        function formatter(modelValue) {
		            if (modelValue && modelValue.Id) {
		                return { Name: modelValue.Name, Id: modelValue.Id, useExisting: true };
		            } else {
		                return { Id: null, Name: '', useExisting: false };
		            }
		        }

		        function parser(viewValue) {
		            if (viewValue.useExisting) {
		                return scope.allActivities.filter(attrValueFilter('Id', viewValue.Id))[0];
		            } else {
		                return { Name: viewValue.Name, Id: null };
		            }
		        }

		        function attrValueFilter(attr, value) {
		            return function (e) {
		                return e[attr] == value;
		            };
		        }
		    }
		}
    ]);

})();