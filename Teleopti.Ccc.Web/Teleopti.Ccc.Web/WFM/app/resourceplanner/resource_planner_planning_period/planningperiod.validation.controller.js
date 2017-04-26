(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('planningPeriodValidationController', Controller)
        .directive('ppValidation', planningperiodValidationDirective);

    Controller.$inject = ['$state', '$stateParams', 'planningPeriodService'];

    function Controller($state, $stateParams, planningPeriodService) {
        var vm = this;
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
