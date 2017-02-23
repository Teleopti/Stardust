(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('dayoffRuleOverviewController', Controller);

    Controller.$inject = ['$state', '$stateParams', 'dayOffRuleService'];

    function Controller($state, $stateParams, dayOffRuleService) {
        var vm = this;

        vm.dayoffRules = [];
        vm.editRuleset = editRuleset;
	    vm.destoryRuleset = destoryRuleset;
	    vm.createRuleset = createRuleset;

        getDayOffRules();

        function getDayOffRules() {
        	return dayOffRuleService.dayoffRules.query().$promise.then(function (data) {
    				vm.dayoffRules = data;
    				return vm.dayoffRules;
    			});
        }

        function editRuleset(dayOffRule) {
			// temporary use the old one, should be rebuilt
        	$state.go('resourceplanner.filter', {
        		filterId: dayOffRule.Id,
        		groupId: $stateParams.groupId,
        		isDefault: dayOffRule.Default,
				periodId: undefined
        	});
        }

        function destoryRuleset(dayOffRule) {
        	if (!dayOffRule.Default) {
				dayOffRuleService.dayoffRules.remove({ id: dayOffRule.Id }).$promise.then(getDayOffRules());
	        }
        }

        function createRuleset() {
        	// temporary use the old one, should be rebuilt
        	$state.go('resourceplanner.filter', {
        		groupId: $stateParams.groupId,
        		periodId: undefined
        	});
        }
    }
})();
