(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupSettingOverviewController', overviewController)
		.controller('schedulingSettingDirectiveOverviewController', directiveController)
		.directive('schedulingSetting', dayoffRulesDirective);

	overviewController.$inject = ['$state', '$timeout', '$stateParams', '$translate', 'PlanGroupSettingService', 'planningGroupInfo', 'schedulingSettingInfo', 'localeLanguageSortingService'];

	function overviewController($state, $timeout, $stateParams, $translate, PlanGroupSettingService, planningGroupInfo, schedulingSettingInfo, localeLanguageSortingService) {
		var vm = this;

		vm.requestSent = false;
		vm.test = false;
		vm.selectedSchedulingSetting = {};
		vm.schedulingSetting = schedulingSettingInfo.sort(localeLanguageSortingService.localeSort('-Priority', '+Name'));
		vm.textDeleteSchedulingSetting = '';
		vm.planningGroupName = planningGroupInfo.Name;
		vm.textOfAppliedFilter = $translate.instant('PlanGroupSchedulingSettingAppliedFilters').replace("{0}", planningGroupInfo.Name);
		vm.getSchedulingSettingInfo = getSchedulingSettingInfo;
		vm.deleteSchedulingSetting = deleteSchedulingSetting;
		vm.goEditSchedulingSetting = goEditSchedulingSetting;
		vm.goCreateSchedulingSetting = goCreateSchedulingSetting;
		vm.setHigherPriority = setHigherPriority;
		vm.setLowerPriority = setLowerPriority;
		vm.disableButton = disableButton;
		vm.setColor = setColor;
		vm.color = {
			render: 'linear',
			rgba: 'rgba(156, 39, 176, 1)'
		};
		vm.preferencePercentage = planningGroupInfo.PreferenceValue * 100;


		getBlockSchedulingSetting();

		function getBlockSchedulingSetting() {
			return vm.schedulingSetting.forEach(function (item) {
				if (item.BlockFinderType > 0) {
					var type = '';
					if(item.BlockSameShiftCategory){
						type = $translate.instant('BlockSameShiftCategory');
					}else if(item.BlockSameStartTime){
						type = $translate.instant('BlockSameStartTime');
					}else if(item.BlockSameShift){
						type = $translate.instant('BlockSameShift');
					}
					if (item.BlockFinderType === 1) {
						item.BlockSchedulingSetting = $translate.instant('BlockFinderTypeBetweenDayOff') + ' ('+type+')';
					} else {
						item.BlockSchedulingSetting = $translate.instant('BlockFinderTypeSchedulePeriod') + ' ('+type+')';
					}
					
				} else {
					item.BlockSchedulingSetting = $translate.instant('Off');
				}
			});
		}

		function setColor(index) {
			if (index == 0) {
				var opacity = 0.05;
			}
			var opacity = 1 - index / vm.schedulingSetting.length;
			return {
				'border-left': '10px solid rgba(156, 39, 176,' + opacity.toFixed(2) + ')'
			}
		}

		function getSchedulingSettingInfo(setting) {
			vm.confirmDeleteModal = true;
			vm.textDeleteSchedulingSetting = $translate.instant('AreYouSureYouWantToDeleteSchedulingSetting').replace("{0}", setting.Name);
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
				groupId: $stateParams.groupId
			});
		}

		function goCreateSchedulingSetting() {
			$state.go('resourceplanner.editsetting', {
				groupId: $stateParams.groupId,
			});
		}

		function setHigherPriority(setting, index) {
			if (setting.Priority == vm.schedulingSetting[0].Priority)
				return;
			addAnimate(index);
			switchPrio(setting, vm.schedulingSetting[index - 1]);
			return resortDisplayOrder(vm.schedulingSetting);
		}

		function setLowerPriority(setting, index) {
			if (setting.Priority < 2)
				return;
			addAnimate(index);
			switchPrio(setting, vm.schedulingSetting[index + 1]);
			return resortDisplayOrder(vm.schedulingSetting);
		}

		function switchPrio(item1, item2) {
			var temp = item1.Priority;
			item1.Priority = item2.Priority;
			item2.Priority = temp;
			persist(item1);
			persist(item2);
			return resortDisplayOrder(vm.schedulingSetting);
		}

		function addAnimate(id) {
			if (id == null || vm.test)
				return;
			var item = document.getElementById(id).parentElement.parentElement;
			item.classList.remove("pg-list-card-animate");
			item.classList.add("pg-list-card-animate");
			return $timeout(function () { item.classList.remove("pg-list-card-animate"); }, 2005);
		}


		function resortDisplayOrder(array) {
			return array.sort(localeLanguageSortingService.localeSort('-Priority', '-Default', '+Name'));
		}

		function disableButton(index) {
			if (index < vm.schedulingSetting.length - 2)
				return false;
			return true;
		}

		function persist(setting) {
			PlanGroupSettingService.saveSetting({
				BlockFinderType: setting.BlockFinderType,
				BlockSameShift: setting.BlockSameShift,
				BlockSameShiftCategory: setting.BlockSameShiftCategory,
				BlockSameStartTime: setting.BlockSameStartTime,
				MinDayOffsPerWeek: setting.MinDayOffsPerWeek,
				MaxDayOffsPerWeek: setting.MaxDayOffsPerWeek,
				MinConsecutiveWorkdays: setting.MinConsecutiveWorkdays,
				MaxConsecutiveWorkdays: setting.MaxConsecutiveWorkdays,
				MinConsecutiveDayOffs: setting.MinConsecutiveDayOffs,
				MaxConsecutiveDayOffs: setting.MaxConsecutiveDayOffs,
				MinFullWeekendsOff: setting.MinFullWeekendsOff,
				MaxFullWeekendsOff: setting.MaxFullWeekendsOff,
				MinWeekendDaysOff: setting.MinWeekendDaysOff,
				MaxWeekendDaysOff: setting.MaxWeekendDaysOff,
				Id: setting.Id,
				Name: setting.Name,
				Default: setting.Default,
				Filters: setting.Filters,
				PlanningGroupId: $stateParams.groupId,
				Priority: setting.Priority
			});
		}
	}

	function directiveController($state, $stateParams, $translate, PlanGroupSettingService, localeLanguageSortingService) {
		var vm = this;

		vm.schedulingSetting = [];
		vm.textOfAppliedFilter = $translate.instant('PlanGroupSchedulingSettingAppliedFilters').replace("{0}", vm.planningGroup.Name);
		vm.color = {
			render: 'linear',
			rgba: 'rgba(156, 39, 176, 1)'
		}

		getDayOffRules();

		function getDayOffRules() {
			return PlanGroupSettingService.getSettingsByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
				vm.schedulingSetting = data.sort(localeLanguageSortingService.localeSort('-Priority', '-Default', '+Name'));
				return getBlockSchedulingSetting();
			});
		}

		function getBlockSchedulingSetting() {
			return vm.schedulingSetting.forEach(function (item) {
				if (item.BlockFinderType > 0) {
					if (item.BlockFinderType == 1) {
						item.BlockSchedulingSetting = $translate.instant('BlockScheduling') + " (" + $translate.instant('BlockFinderTypeBetweenDayOff') + ")";
					} else {
						item.BlockSchedulingSetting = $translate.instant('BlockScheduling') + " (" + $translate.instant('BlockFinderTypeSchedulePeriod') + ")";
					}
				} else {
					item.BlockSchedulingSetting = $translate.instant('IndividualFlexible') + " (" + $translate.instant('Default') + ")";
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
