(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodValidationController', Controller)
        .directive('ppValidation', planningperiodValidationDirective);

    Controller.$inject = ['$stateParams', '$translate', '$timeout', 'planningPeriodServiceNew'];

    function Controller($stateParams, $translate, $timeout, planningPeriodServiceNew) {
        var vm = this;

        vm.message = "";
        vm.valData = {
            totalValNum: 0,
            totalPreValNum: 0,
            scheduleIssues: [],
            preValidation: []
        };
        vm.getValidationByPpId = getValidationByPpId;

        getValidationByPpId();

        function getValidationByPpId() {
            if ($stateParams.ppId == null)
                return;
            planningPeriodServiceNew.getValidation({ id: $stateParams.ppId })
                .$promise.then(function (data) {
                    vm.valData.preValidation = data.InvalidResources;
                    getTotalValidationErrorsNumber();
                });
        }

        function getTotalValidationErrorsNumber() {
            vm.valData.totalValNum = 0;
            vm.valData.totalPreValNum = 0;
            var pre = vm.valData.preValidation;
            var after = vm.valData.scheduleIssues;

            if (pre.length > 0) {
                angular.forEach(pre, function (item) {
                    vm.valData.totalPreValNum += item.ValidationErrors.length;
                });
            }
            if (after.length > 0) {
                vm.valData.totalValNum += after.length;
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
