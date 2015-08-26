(function() {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundViewUtilityService', viewUtilityService);

    function viewUtilityService() {

        this.registerCampaignForms = registerCampaignForms;
        this.registerPersonHourFeedback = registerPersonHourFeedback;
        this.setupValidators = setupValidators;
        this.expandAllSections = expandAllSections;
        this.deepPropertyAccess = deepPropertyAccess;

        function registerCampaignForms(scope) {
            scope.$on('formLocator.campaignGeneralForm', function (event) {
                scope.campaignGeneralForm = event.targetScope.campaignGeneralForm;
                scope.formScope = event.targetScope;
                scope.campaignGeneralForm.$setPristine();
            });

            scope.$on('formLocator.campaignWorkloadForm', function (event) {
                scope.campaignWorkloadForm = event.targetScope.campaignWorkloadForm;
                scope.formScope = event.targetScope;
                scope.campaignWorkloadForm.$setPristine();
            });

            scope.$on('formLocator.campaignSpanningPeriodForm', function (event) {
                scope.campaignSpanningPeriodForm = event.targetScope.campaignSpanningPeriodForm;
                scope.formScope = event.targetScope;
                scope.campaignSpanningPeriodForm.$setPristine();
            });
        }

        function registerPersonHourFeedback(scope, outboundService) {
            scope.$watch(function () {
                return scope.campaignWorkloadForm && scope.campaignWorkloadForm.$valid ?
                    outboundService.calculateCampaignPersonHour(scope.campaign) : null;
            }, function (newValue) {
                scope.estimatedWorkload = newValue;
            });
        }

        function setupValidators(scope) {
            scope.isInputValid = isInputValid;
            scope.validWorkingHours = validWorkingHours;
            scope.flashErrorIcons = flashErrorIcons;
            scope.setRangeClass = setDateRangeClass;

            scope.$watch(function () {
                var startDate = deepPropertyAccess(scope, ['campaign', 'StartDate', 'Date']);
                var endDate = deepPropertyAccess(scope, ['campaign', 'EndDate', 'Date']);
                return { startDate: startDate, endDate: endDate };
            }, function (value) {
                if (scope.campaignSpanningPeriodForm) {
                    if (!isDateValid(value.startDate, value.endDate)) {
                        scope.campaignSpanningPeriodForm.$setValidity('order', false);
                    } else {
                        scope.campaignSpanningPeriodForm.$setValidity('order', true);
                    }
                }
                scope.campaign.StartDate = { Date: angular.copy(value.startDate) };
                scope.campaign.EndDate = { Date: angular.copy(value.endDate) };
            }, true);

            function isInputValid() {
                if (!scope.campaignGeneralForm) return false;
                if (!scope.campaignWorkloadForm) return false;
                if (!scope.campaignSpanningPeriodForm) return false;

                return scope.campaignGeneralForm.$valid
                    && scope.campaignWorkloadForm.$valid
                    && scope.campaignSpanningPeriodForm.$valid
                    && validWorkingHours();
            }

            function isDateValid(startDate, endDate) {
                return endDate && startDate && startDate <= endDate;
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

            function setDateRangeClass(date, mode) {
                if (mode === 'day') {
                    var startDate = deepPropertyAccess(scope, ['campaign', 'StartDate', 'Date']);
                    var endDate = deepPropertyAccess(scope, ['campaign', 'EndDate', 'Date']);

                    if (startDate && endDate && startDate <= endDate) {
                        var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
                        var start = new Date(startDate).setHours(12, 0, 0, 0);
                        var end = new Date(endDate).setHours(12, 0, 0, 0);

                        if (dayToCheck >= start && dayToCheck <= end) {
                            return 'in-date-range';
                        }
                    }
                }
                return '';
            }

            function flashErrorIcons() {
                scope.$broadcast('expandable.expand');
            }
        }

        function expandAllSections(scope) {
            scope.acToggle0 = true;
            scope.acToggle1 = true;
            scope.acToggle2 = true;
            scope.acToggle3 = true;
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