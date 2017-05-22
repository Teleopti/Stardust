(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('dayoffRuleOverviewController', overviewController)
		.controller('dayoffRuleDirectiveOverviewController', directiveController)
		.directive('dayoffRules', dayoffRulesDirective);

	overviewController.$inject = ['$state', '$stateParams', '$translate', 'dayOffRuleService', 'agentGroupInfo', 'dayOffRulesInfo'];

	function overviewController($state, $stateParams, $translate, dayOffRuleService, agentGroupInfo, dayOffRulesInfo) {
		var vm = this;

		vm.dayOffRules = dayOffRulesInfo ? dayOffRulesInfo : [];
		vm.textDeleteDoRule = '';
		vm.textManageDoRule = $translate.instant("ManageDayOffForAgentGroup").replace("{0}", agentGroupInfo.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", agentGroupInfo.Name);
		vm.getDoRuleInfo = getDoRuleInfo;
		vm.deleteDoRule = deleteDoRule;
		vm.goEditDoRule = goEditDoRule;
		vm.goCreateDoRule = goCreateDoRule;

		function getDoRuleInfo(dayOffRule) {
			vm.textDeleteDoRule = $translate.instant("AreYouSureYouWantToDeleteTheDayOffRule").replace("{0}", dayOffRule.Name);
		}

		function deleteDoRule(dayOffRule) {
			if (dayOffRule.Default)
				return;
			var deleteDayOffRule = dayOffRuleService.removeDayOffRule({ id: dayOffRule.Id });
			return deleteDayOffRule.$promise.then(function () {
				var index = vm.dayOffRules.indexOf(dayOffRule);
				vm.dayOffRules.splice(index, 1);
			});
		}

		function goEditDoRule(dayOffRule) {
			$state.go('resourceplanner.dayoffrule', {
				filterId: dayOffRule.Id.toString(),
				groupId: $stateParams.groupId,
				isDefault: dayOffRule.Default
			});
		}

		function goCreateDoRule() {
			$state.go('resourceplanner.dayoffrule', {
				groupId: $stateParams.groupId
			});
		}
	}

	function directiveController($state, $stateParams, $translate, dayOffRuleService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.textManageDoRule = $translate.instant("ManageDayOffForAgentGroup").replace("{0}", vm.agentGroup.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", vm.agentGroup.Name);
		vm.goDoRulesSetting = goDoRulesSetting;

		getDayOffRules();

		function getDayOffRules() {
			return dayOffRuleService.getDayOffRulesByAgentGroupId({ agentGroupId: $stateParams.groupId }).$promise.then(function (data) {
				return vm.dayOffRules = data;
			});
		}

		function goDoRulesSetting() {
			$state.go('resourceplanner.dayoffrulesoverview', {
				groupId: $stateParams.groupId,
			});
		}
	}

	function dayoffRulesDirective() {
		var directive = {
			restrict: 'EA',
			scope: {
				isDisable: '=',
				agentGroup: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleDirectiveOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
