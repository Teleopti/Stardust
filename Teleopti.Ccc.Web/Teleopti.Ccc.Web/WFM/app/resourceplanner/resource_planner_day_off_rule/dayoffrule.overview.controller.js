(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('dayoffRuleOverviewController', Controller)
		.directive('dayoffRules', dayoffRulesDirective);

	Controller.$inject = ['$state', '$stateParams', 'dayOffRuleService', 'agentGroupService'];

	function Controller($state, $stateParams, dayOffRuleService, agentGroupService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.agentGroup = {};
		vm.editRuleset = editRuleset;
		vm.destoryRuleset = destoryRuleset;
		vm.createRuleset = createRuleset;
		vm.goDayoffRuleSetting = goDayoffRuleSetting;


		getDayOffRules();
		getAgentGroupbyId();

		function getAgentGroupbyId() {
			if ($stateParams.groupId !== null) {
				var getAgentGroup = agentGroupService.getAgentGroupbyId({ id: $stateParams.groupId });
				return getAgentGroup.$promise.then(function (data) {
					vm.agentGroup = data;
					return vm.agentGroup;
				});
			}
		}

		function getDayOffRules() {
			if ($stateParams.groupId == null) {
				return;
			}
			return dayOffRuleService.getDayOffRulesForAgentGroup({ agentGroupId: $stateParams.groupId })
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

		function goDayoffRuleSetting() {
			$state.go('resourceplanner.dayoffrulesOverview', {
				groupId: $stateParams.groupId,
			});
		}
	}

	function dayoffRulesDirective() {
		var directive = {
			restrict: 'EA',
			scope: {
				isDisable: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
