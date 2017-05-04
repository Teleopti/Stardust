(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodValidationController', Controller)
        .directive('ppValidation', planningperiodValidationDirective);

    Controller.$inject = ['$state', '$stateParams', '$translate', '$timeout', 'planningPeriodService'];

    function Controller($state, $stateParams, $translate, $timeout, planningPeriodService) {
        var vm = this;

        vm.message = "";
        vm.updatePreValidation = updatePreValidation;

        function updatePreValidation() {
            var preValidation = planningPeriodService.getPlanningPeriod({ id: vm.valData.selectedPpId });
            return preValidation.$promise.then(function (data) {
                vm.valData.preValidation = data.ValidationResult.InvalidResources;
                getTotalValidationErrorsNumber(vm.valData.preValidation, vm.valData.scheduleIssues);
                vm.message = $translate.instant("UpdatedValidation");
                $timeout(function () {
                    vm.message = "";
                }, 5000);
                return vm.valData;
            });
        }

        function getTotalValidationErrorsNumber(pre, after) {
            vm.valData.totalValNum = 0;
            vm.valData.totalPreValNum = 0;
            if (pre.length > 0) {
                angular.forEach(pre, function (item) {
                    vm.valData.totalPreValNum += item.ValidationErrors.length;
                });
            }
            if (after.length > 0) {
                vm.valData.totalValNum += vm.valData.scheduleIssues.length;
            }
            return vm.valData.totalValNum += vm.valData.totalPreValNum;
        }
    }

    function planningperiodValidationDirective() {
        var directive = {
            restrict: 'EA',
            scope: {
                valData: '='
            },
            templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.validation.html',
            controller: 'planningPeriodValidationController as vm',
            bindToController: true
        };
        return directive;
    }
})();
