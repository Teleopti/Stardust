(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('dayoffRuleOverviewController', Controller)
		.directive('dayoffRules', dayoffRulesDirective);

	Controller.$inject = ['$state', '$stateParams', '$translate', 'dayOffRuleService', 'agentGroupService'];

	function Controller($state, $stateParams, $translate, dayOffRuleService, agentGroupService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.textManageDoRule = '';
		vm.textDeleteDoRule = '';
		vm.textDoRuleAppliedFilter = '';
		vm.getDoRuleInfo = getDoRuleInfo;
		vm.deleteDoRule = deleteDoRule;
		vm.goEditDoRule = goEditDoRule;
		vm.goCreateDoRule = goCreateDoRule;
		vm.goDoRulesSetting = goDoRulesSetting;

		getDayOffRules();
		getAgentGroupById();

		function getAgentGroupById() {
			if ($stateParams.groupId) {
				vm.agentGroup = {};
				var getAgentGroup = agentGroupService.getAgentGroupById({ id: $stateParams.groupId });
				return getAgentGroup.$promise.then(function (data) {
					vm.agentGroup = data;
					textForAgentGroup();
					return vm.agentGroup;
				});
			}

		}

		function getDayOffRules() {
			if ($stateParams.groupId) {
				var dayOffRule = dayOffRuleService.getDayOffRulesByAgentGroupId({ agentGroupId: $stateParams.groupId });
				return dayOffRule.$promise.then(function (data) {
					vm.dayOffRules = data;
					return vm.dayOffRules;
				});
			}
		}

		function textForAgentGroup() {
			vm.textManageDoRule = $translate.instant("ManageDayOffForAgentGroup").replace("{0}", vm.agentGroup.Name);
			vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", vm.agentGroup.Name);
		}

		function getDoRuleInfo(dayOffRule) {
			vm.textDeleteDoRule = $translate.instant("AreYouSureYouWantToDeleteTheDayOffRule").replace("{0}", dayOffRule.Name);
		}

		function deleteDoRule(dayOffRule) {
			if (!dayOffRule.Default) {
				var deleteDayOffRule = dayOffRuleService.removeDayOffRule({ id: dayOffRule.Id });
				return deleteDayOffRule.$promise.then(function () {
					var index = vm.dayOffRules.indexOf(dayOffRule);
					vm.dayOffRules.splice(index, 1);
				});
			}
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
			controller: 'dayoffRuleOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
