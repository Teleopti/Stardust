(function() {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundViewUtilityService', viewUtilityService);

    function viewUtilityService() {
        this.registerPersonHourFeedback = registerPersonHourFeedback;
        this.setupValidators = setupValidators;
       // this.expandAllSections = expandAllSections;
        this.deepPropertyAccess = deepPropertyAccess;

        //function registerCampaignForms(scope) {
        //    scope.$on('formLocator.campaignGeneralForm', function (event) {
        //        scope.campaignGeneralForm = event.targetScope.campaignGeneralForm;
        //        scope.formScope = event.targetScope;
        //        scope.campaignGeneralForm.$setPristine();
        //    });

        //    scope.$on('formLocator.campaignWorkloadForm', function (event) {
        //        scope.campaignWorkloadForm = event.targetScope.campaignWorkloadForm;
        //        scope.formScope = event.targetScope;
        //        scope.campaignWorkloadForm.$setPristine();
        //    });

        //    scope.$on('formLocator.campaignSpanningPeriodForm', function (event) {
        //        scope.campaignSpanningPeriodForm = event.targetScope.campaignSpanningPeriodForm;
        //        scope.formScope = event.targetScope;
        //        scope.campaignSpanningPeriodForm.$setPristine();
        //    });
        //}

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
						//scope.campaignSpanningPeriodForm.$setValidity(v, true);
					});
				}
	        	if (newValue && newValue.length > 0) {
	        		angular.forEach(newValue, function (v) {
				        campaignDurationError.push(v);
				        //scope.campaignSpanningPeriodForm.$setValidity(v, false);
			        });
				}
	        }, true);

	        function isInputValid() {
	        	console.log('scope.form', scope.form);
	        	if (scope.form.$error.required && scope.form.$error.required.length > 0) return false; //|| campaignDurationError.length>0
		        if (campaignDurationError.length > 0) return false;
		        // if (!scope.campaignWorkloadForm) return false;
		        // if (!scope.campaignSpanningPeriodForm) return false;

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

        //function expandAllSections(scope) {
        //    scope.acToggle0 = true;
        //    scope.acToggle1 = true;
        //    scope.acToggle2 = true;
        //    scope.acToggle3 = true;
        //}

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