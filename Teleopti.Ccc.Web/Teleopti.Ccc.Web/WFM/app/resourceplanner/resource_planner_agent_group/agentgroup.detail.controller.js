(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('agentGroupsDetailController', Controller)
        .directive('planingPeriods', planingPeriodsDirective);
        // .directive('dayoffRules', dayoffRulesDirective);

    Controller.$inject = ['$stateParams'];

    /* @ngInject */
    function Controller($stateParams) {
        var vm = this;

    }

    function planingPeriodsDirective() {
        var directive = {
            restrict: 'EA',
            templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.overview.html',
            controller: 'planningPeriodOverviewController as vm',
            bindToController: true
        };
        return directive;
    }

    // function dayoffRulesDirective() {
    //     var directive = {
    //         restrict: 'EA',
    //         templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
    //         controller: 'dayoffRuleOverviewController as vm',
    //         bindToController: true
    //     };
    //     return directive;
    // }
})();
