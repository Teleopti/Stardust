(function () {
	'use strict';

	angular
        .module('wfm.resourceplanner')
        .controller('dayoffRuleOverviewController', Controller)
		.directive('dayoffRules', dayoffRulesDirective);

	Controller.$inject = ['$state', '$stateParams', 'dayOffRuleService'];

	function Controller($state, $stateParams, dayOffRuleService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.editRuleset = editRuleset;
		vm.destoryRuleset = destoryRuleset;
		vm.createRuleset = createRuleset;

		getDayOffRules();

		function getDayOffRules() {
			return dayOffRuleService.getDayOffRulesForAgentGroup({ agentGroupId: $stateParams.groupId})
				.$promise.then(function (data) {
					vm.dayOffRules = data;
					return vm.dayOffRules;
				});
		}

		function editRuleset(dayOffRule) {
			// temporary use the old one, should be rebuilt; resourceplanner.filter => resourceplanner.dayoffrules
			$state.go('resourceplanner.dayoffrules', {
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
			// temporary use the old one, should be rebuilt; resourceplanner.filter => resourceplanner.dayoffrules
			$state.go('resourceplanner.dayoffrules', {
				groupId: $stateParams.groupId,
				periodId: undefined
			});
		}
	}

	function dayoffRulesDirective() {
		var directive = {
			restrict: 'EA',
      scope: {},
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
