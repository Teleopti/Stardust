(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('dayoffRuleOverviewController', overviewController)
		.controller('dayoffRuleDirectiveOverviewController', directiveController)
		.directive('dayoffRules', dayoffRulesDirective);

	overviewController.$inject = ['$state', '$stateParams', '$translate', 'dayOffRuleService', 'planningGroupInfo', 'dayOffRulesInfo', 'localeLanguageSortingService'];

	function overviewController($state, $stateParams, $translate, dayOffRuleService, planningGroupInfo, dayOffRulesInfo, localeLanguageSortingService) {
		var vm = this;

		vm.dayOffRules = dayOffRulesInfo.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
		vm.textDeleteDoRule = '';
		vm.textManageDoRule = $translate.instant("ManageDayOffForPlanningGroup").replace("{0}", planningGroupInfo.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", planningGroupInfo.Name);
		vm.getDoRuleInfo = getDoRuleInfo;
		vm.deleteDoRule = deleteDoRule;
		vm.goEditDoRule = goEditDoRule;
		vm.goCreateDoRule = goCreateDoRule;

		function getDoRuleInfo(dayOffRule) {
			vm.textDeleteDoRule = $translate.instant("AreYouSureYouWantToDeleteTheDayOffRule").replace("{0}", dayOffRule.Name);
		}

		function deleteDoRule(dayOffRule) {
			if (dayOffRule.Default == true)
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
				isDefault: dayOffRule.Default,
				EditDoRule: true
			});
		}

		function goCreateDoRule() {
			$state.go('resourceplanner.dayoffrule', {
				groupId: $stateParams.groupId,
				EditDoRule: false
			});
		}
	}

	function directiveController($state, $stateParams, $translate, dayOffRuleService, localeLanguageSortingService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.textManageDoRule = $translate.instant("ManageDayOffForPlanningGroup").replace("{0}", vm.planningGroup.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", vm.planningGroup.Name);

		getDayOffRules();

		function getDayOffRules() {
			return dayOffRuleService.getDayOffRulesByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
				vm.dayOffRules = data.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
				return vm.dayOffRules;
			});
		}
	}

	function dayoffRulesDirective() {
		var directive = {
			restrict: 'EA',
			scope: {
				isDisable: '=',
				planningGroup: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleDirectiveOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
