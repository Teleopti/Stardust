(function() {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundViewUtilityService', viewUtilityService);

    function viewUtilityService() {
        this.registerPersonHourFeedback = registerPersonHourFeedback;
        this.setupValidators = setupValidators;
        this.deepPropertyAccess = deepPropertyAccess;

        function registerPersonHourFeedback(scope, outboundService) {
            scope.$watch(function () {
            	return (scope.form.$error.required && scope.form.$error.required.length == 1 && scope.form.$error.required[0].$name == 'Name') || scope.form.$error.required ==undefined ? //scope.campaignWorkloadForm && scope.campaignWorkloadForm.$valid ?
            	    outboundService.calculateCampaignPersonHour(scope.campaign) : null;
            }, function (newValue) {
                scope.estimatedWorkload = newValue;
            });
        }

        function setupValidators(scope) {
            scope.isInputValid = isInputValid;
            scope.validWorkingHours = validWorkingHours;
            scope.flashErrorIcons = flashErrorIcons;
	        var campaignDurationError = [];

	        scope.$watch(function() {
		        return scope.campaign.spanningPeriodErrors;
	        }, function (newValue, oldValue) {		        
				if (oldValue && oldValue.length > 0) {
					angular.forEach(oldValue, function (v) {
						campaignDurationError = [];
					});
				}
	        	if (newValue && newValue.length > 0) {
	        		angular.forEach(newValue, function (v) {
				        campaignDurationError.push(v);
			        });
				}
	        }, true);

	        function isInputValid() {
	        	if (scope.form.$error.required && scope.form.$error.required.length > 0) return false;
		        if (campaignDurationError.length > 0) return false;
		        return  validWorkingHours();
	        }          

            function validWorkingHours() {
                var i, j;
                for (i = 0; i < scope.campaign.WorkingHours.length; i++) {
                    for (j = 0; j < scope.campaign.WorkingHours[i].WeekDaySelections.length; j++) {
                        if (scope.campaign.WorkingHours[i].WeekDaySelections[j].Checked) return true;
                    }
                }
                return false;
            }

            function flashErrorIcons() {
                scope.$broadcast('expandable.expand');
            }
        }

        function deepPropertyAccess(obj, propertiesChain) {
            var curObj = obj;
            for (var i = 0; i < propertiesChain.length; i += 1) {
                var curProperty = propertiesChain[i];
                if (angular.isDefined(curObj[curProperty])) curObj = curObj[curProperty];
                else return;
            }
            return curObj;
        }

    }   
})();