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
		vm.textManageDoRule = $translate.instant("ManagePlanningGroupSchedulingSetting").replace("{0}", planningGroupInfo.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("PlanGroupSchedulingSettingAppliedFilters").replace("{0}", planningGroupInfo.Name);
		vm.getDoRuleInfo = getDoRuleInfo;
		vm.deleteDoRule = deleteDoRule;
		vm.goEditDoRule = goEditDoRule;
		vm.goCreateDoRule = goCreateDoRule;

		getBlockSchedulingSetting();

		function getBlockSchedulingSetting() {
			return vm.dayOffRules.forEach(function (item) {
				if (item.BlockFinderType > 0) {
					if (item.BlockFinderType == 1) {
						item.BlockSchedulingSetting = $translate.instant("BlockScheduling") + " (" + $translate.instant("BlockFinderTypeBetweenDayOff") + ")";
					} else {
						item.BlockSchedulingSetting = $translate.instant("BlockScheduling") + " (" + $translate.instant("BlockFinderTypeSchedulePeriod") + ")";
					}
				} else {
					item.BlockSchedulingSetting = $translate.instant("IndividualFlexible") + " (" + $translate.instant("Default") + ")";
				}
			});
		}

		function getDoRuleInfo(dayOffRule) {
			vm.confirmDeleteModal = true;
			vm.textDeleteDoRule = $translate.instant("AreYouSureYouWantToDeleteSchedulingSetting").replace("{0}", dayOffRule.Name);
			return vm.selectedDayOffRule = dayOffRule;
		}

		function deleteDoRule() {
			if (vm.selectedDayOffRule.Default == true || vm.requestSent)
				return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				var deleteDayOffRule = PlanGroupSettingService.removeSetting({ id: vm.selectedDayOffRule.Id });
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
			});
		}

		function goCreateDoRule() {
			$state.go('resourceplanner.editsetting', {
				groupId: $stateParams.groupId,
			});
		}
	}

	function directiveController($state, $stateParams, $translate, PlanGroupSettingService, localeLanguageSortingService) {
		var vm = this;

		vm.dayOffRules = [];
		vm.textManageDoRule = $translate.instant("ManagePlanningGroupSchedulingSetting").replace("{0}", vm.planningGroup.Name);
		vm.textDoRuleAppliedFilter = $translate.instant("PlanGroupSchedulingSettingAppliedFilters").replace("{0}", vm.planningGroup.Name);

		getDayOffRules();
		
		function getDayOffRules() {
			return PlanGroupSettingService.getSettingsByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
				vm.dayOffRules = data.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
				return getBlockSchedulingSetting();
			});
		}

		function getBlockSchedulingSetting() {
			return vm.dayOffRules.forEach(function (item) {
				if (item.BlockFinderType > 0) {
					if (item.BlockFinderType == 1) {
						item.BlockSchedulingSetting = $translate.instant("BlockScheduling") + " (" + $translate.instant("BlockFinderTypeBetweenDayOff") + ")";
					} else {
						item.BlockSchedulingSetting = $translate.instant("BlockScheduling") + " (" + $translate.instant("BlockFinderTypeSchedulePeriod") + ")";
					}
				} else {
					item.BlockSchedulingSetting = $translate.instant("IndividualFlexible") + " (" + $translate.instant("Default") + ")";
				}
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
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.overview.html',
			controller: 'dayoffRuleDirectiveOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
