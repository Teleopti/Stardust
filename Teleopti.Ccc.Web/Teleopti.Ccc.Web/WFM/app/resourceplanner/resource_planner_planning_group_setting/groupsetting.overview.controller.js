(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupSettingOverviewController', overviewController)
		.directive('planningGroupSettingsOverview', planningGroupSettingOverviewDirective);

	overviewController.$inject = ['$state', '$timeout', '$stateParams', '$translate', 'localeLanguageSortingService'];

	function overviewController($state, $timeout, $stateParams, $translate, localeLanguageSortingService) {
		var vm = this;

		vm.requestSent = false;
		vm.test = false;
		vm.selectedSchedulingSetting = {};
		vm.settings = vm.settings.sort(localeLanguageSortingService.localeSort('-Priority', '+Name'));
		vm.textDeleteSchedulingSetting = '';
		vm.showDeleteSchedulingSettingModal = showDeleteSchedulingSettingModal;
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

		function setColor(index) {
			var opacity = 1 - index / vm.settings.length;
			if (index === 0) {
				opacity = 0.05;
			}
			return {
				'border-left': '10px solid rgba(156, 39, 176,' + opacity.toFixed(2) + ')'
			}
		}

		function showDeleteSchedulingSettingModal(setting) {
			vm.confirmDeleteModal = true;
			vm.textDeleteSchedulingSetting = $translate.instant('AreYouSureYouWantToDeleteSchedulingSetting').replace("{0}", setting.Name);
			return vm.selectedSchedulingSetting = setting;
		}

		function deleteSchedulingSetting() {
			if (vm.selectedSchedulingSetting.Default || vm.requestSent)
				return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				var deleteDayOffRule = PlanGroupSettingService.removeSetting({ id: vm.selectedSchedulingSetting.Id });
				return deleteDayOffRule.$promise.then(function () {
					var index = vm.settings.indexOf(vm.selectedSchedulingSetting);
					vm.settings.splice(index, 1);
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
				groupId: $stateParams.groupId
			});
		}

		function setHigherPriority(setting, index) {
			if (setting.Priority === vm.settings[0].Priority)
				return;
			addAnimate(index);
			switchPrio(setting, vm.settings[index - 1]);
			return resortDisplayOrder(vm.settings);
		}

		function setLowerPriority(setting, index) {
			if (setting.Priority < 2)
				return;
			addAnimate(index);
			switchPrio(setting, vm.settings[index + 1]);
			return resortDisplayOrder(vm.settings);
		}

		function switchPrio(item1, item2) {
			var temp = item1.Priority;
			item1.Priority = item2.Priority;
			item2.Priority = temp;
			persist(item1);
			persist(item2);
			return resortDisplayOrder(vm.settings);
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
			return index >= vm.settings.length - 2;
		}
	}

	function planningGroupSettingOverviewDirective() {
		return {
			restrict: 'EA',
			scope: {
				settings: '=',
				preferencePercent: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.overview.html',
			controller: 'planningGroupSettingOverviewController as vm',
			bindToController: true
		};
	}
})();
