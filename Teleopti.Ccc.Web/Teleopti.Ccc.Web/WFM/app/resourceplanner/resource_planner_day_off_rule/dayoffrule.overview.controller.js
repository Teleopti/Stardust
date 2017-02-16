(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('dayoffRuleOverviewController', Controller);

    Controller.$inject = ['dayOffRuleService'];

    function Controller(dayOffRuleService) {
        var vm = this;

        vm.dayoffRules = [];

        getDayOffRules();

        function getDayOffRules(){
          var getDayOffRules = dayOffRuleService.getDayOffRules.query();
    			return getDayOffRules.$promise.then(function(data) {
    				vm.dayoffRules = data;

    				return vm.dayoffRules;
    			});
        }
    }
})();
