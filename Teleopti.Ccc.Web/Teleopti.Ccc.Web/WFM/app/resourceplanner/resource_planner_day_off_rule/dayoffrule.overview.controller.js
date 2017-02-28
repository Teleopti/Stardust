(function () {
	'use strict';

	angular
        .module('wfm.resourceplanner')
        .controller('dayoffRuleOverviewController', Controller);

	Controller.$inject = ['$state', '$stateParams', 'dayOffRuleService'];

	function Controller($state, $stateParams, dayOffRuleService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.editRuleset = editRuleset;
		vm.destoryRuleset = destoryRuleset;
		vm.createRuleset = createRuleset;

		getDayOffRules();

		function getDayOffRules() {
			return dayOffRuleService.getDayOffRules()
				.$promise.then(function (data) {
					vm.dayOffRules = data;
					return vm.dayOffRules;
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
				dayOffRuleService.removeDayOffRule({ id: dayOffRule.Id })
					.$promise.then(getDayOffRules);
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
