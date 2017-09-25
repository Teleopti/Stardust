(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupSettingOverviewController', overviewController)
		.controller('dayoffRuleDirectiveOverviewController', directiveController)
		.directive('dayoffRules', dayoffRulesDirective);

	overviewController.$inject = ['$state', '$stateParams', '$translate', 'PlanGroupSettingService', 'planningGroupInfo', 'dayOffRulesInfo', 'localeLanguageSortingService'];

	function overviewController($state, $stateParams, $translate, PlanGroupSettingService, planningGroupInfo, dayOffRulesInfo, localeLanguageSortingService) {
		var vm = this;

		vm.requestSent = false;
		vm.selectedDayOffRule = {};
		vm.dayOffRules = dayOffRulesInfo.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
		vm.textDeleteDoRule = '';
		vm.textManageDoRule = $translate.instant("ManageDayOffForPlanningGroup").replace("{0}", planningGroupInfo.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", planningGroupInfo.Name);
		vm.getDoRuleInfo = getDoRuleInfo;
		vm.deleteDoRule = deleteDoRule;
		vm.goEditDoRule = goEditDoRule;
		vm.goCreateDoRule = goCreateDoRule;

		function getDoRuleInfo(dayOffRule) {
			vm.confirmDeleteModal = true;
			vm.textDeleteDoRule = $translate.instant("AreYouSureYouWantToDeleteTheDayOffRule").replace("{0}", dayOffRule.Name);
			return vm.selectedDayOffRule = dayOffRule;
		}

		function deleteDoRule() {
			if (vm.selectedDayOffRule.Default == true || vm.requestSent)
				return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				var deleteDayOffRule = PlanGroupSettingService.removeDayOffRule({ id: vm.selectedDayOffRule.Id });
				return deleteDayOffRule.$promise.then(function () {
					var index = vm.dayOffRules.indexOf(vm.selectedDayOffRule);
					vm.dayOffRules.splice(index, 1);
					vm.confirmDeleteModal = false;
					vm.requestSent = false;
				});
			}
		}

		function goEditDoRule(dayOffRule) {
			$state.go('resourceplanner.editsetting', {
				filterId: dayOffRule.Id.toString(),
				groupId: $stateParams.groupId,
				isDefault: dayOffRule.Default,
				EditDoRule: true
			});
		}

		function goCreateDoRule() {
			$state.go('resourceplanner.editsetting', {
				groupId: $stateParams.groupId,
				EditDoRule: false
			});
		}
	}

	function directiveController($state, $stateParams, $translate, PlanGroupSettingService, localeLanguageSortingService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.textManageDoRule = $translate.instant("ManageDayOffForPlanningGroup").replace("{0}", vm.planningGroup.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("DayOffRuleAppliedFilters").replace("{0}", vm.planningGroup.Name);

		getDayOffRules();

		function getDayOffRules() {
			return PlanGroupSettingService.getDayOffRulesByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
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
