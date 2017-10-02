(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupSettingOverviewController', overviewController)
		.controller('schedulingSettingDirectiveOverviewController', directiveController)
		.directive('schedulingSetting', dayoffRulesDirective);

	overviewController.$inject = ['$state', '$stateParams', '$translate', 'PlanGroupSettingService', 'planningGroupInfo', 'schedulingSettingInfo', 'localeLanguageSortingService'];

	function overviewController($state, $stateParams, $translate, PlanGroupSettingService, planningGroupInfo, schedulingSettingInfo, localeLanguageSortingService) {
		var vm = this;

		vm.requestSent = false;
		vm.selectedSchedulingSetting = {};
		vm.schedulingSetting = schedulingSettingInfo.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
		vm.textDeleteSchedulingSetting = '';
		vm.textManageSchedulingSetting = $translate.instant("ManagePlanningGroupSchedulingSetting").replace("{0}", planningGroupInfo.Name);
		vm.textOfAppliedFilter = $translate.instant("PlanGroupSchedulingSettingAppliedFilters").replace("{0}", planningGroupInfo.Name);
		vm.getSchedulingSettingInfo = getSchedulingSettingInfo;
		vm.deleteSchedulingSetting = deleteSchedulingSetting;
		vm.goEditSchedulingSetting = goEditSchedulingSetting;
		vm.goCreateSchedulingSetting = goCreateSchedulingSetting;

		getBlockSchedulingSetting();

		function getBlockSchedulingSetting() {
			return vm.schedulingSetting.forEach(function (item) {
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

		function getSchedulingSettingInfo(setting) {
			vm.confirmDeleteModal = true;
			vm.textDeleteSchedulingSetting = $translate.instant("AreYouSureYouWantToDeleteSchedulingSetting").replace("{0}", setting.Name);
			return vm.selectedSchedulingSetting = setting;
		}

		function deleteSchedulingSetting() {
			if (vm.selectedSchedulingSetting.Default == true || vm.requestSent)
				return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				var deleteDayOffRule = PlanGroupSettingService.removeSetting({ id: vm.selectedSchedulingSetting.Id });
				return deleteDayOffRule.$promise.then(function () {
					var index = vm.schedulingSetting.indexOf(vm.selectedSchedulingSetting);
					vm.schedulingSetting.splice(index, 1);
					vm.confirmDeleteModal = false;
					vm.requestSent = false;
				});
			}
		}

		function goEditSchedulingSetting(setting) {
			$state.go('resourceplanner.editsetting', {
				filterId: setting.Id.toString(),
				groupId: $stateParams.groupId,
				isDefault: setting.Default,
			});
		}

		function goCreateSchedulingSetting() {
			$state.go('resourceplanner.editsetting', {
				groupId: $stateParams.groupId,
			});
		}
	}

	function directiveController($state, $stateParams, $translate, PlanGroupSettingService, localeLanguageSortingService) {
		var vm = this;

		vm.schedulingSetting = [];
		vm.textManageSchedulingSetting = $translate.instant("ManagePlanningGroupSchedulingSetting").replace("{0}", vm.planningGroup.Name);
		vm.textOfAppliedFilter = $translate.instant("PlanGroupSchedulingSettingAppliedFilters").replace("{0}", vm.planningGroup.Name);

		getDayOffRules();

		function getDayOffRules() {
			return PlanGroupSettingService.getSettingsByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
				vm.schedulingSetting = data.sort(localeLanguageSortingService.localeSort('-Default', '+Name'));
				return getBlockSchedulingSetting();
			});
		}

		function getBlockSchedulingSetting() {
			return vm.schedulingSetting.forEach(function (item) {
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
			controller: 'schedulingSettingDirectiveOverviewController as vm',
			bindToController: true
		};
		return directive;
	}
})();
