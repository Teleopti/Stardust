(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$stateParams', 'planningGroupService', 'NoticeService', '$translate', 'debounceService', 'localeLanguageSortingService', 'editPlanningGroup'];

	function Controller($state, $timeout, $stateParams, planningGroupService, NoticeService, $translate, debounceService, localeLanguageSortingService, editPlanningGroup) {
		var vm = this;

		vm.requestSent = false;
		vm.searchString = '';
		vm.selectedResults = [];
		vm.filterResults = [];
		vm.name = '';
		vm.cancel = returnToPreviousView;
		vm.deletePlanningGroupText = '';
		vm.isEditGroup = !!editPlanningGroup;
		vm.editPlanningGroup = editPlanningGroup? editPlanningGroup:{
			PreferencePercent: 80,
			Settings:[{
				BlockFinderType: 0,
				BlockSameShift: false,
				BlockSameShiftCategory: false,
				BlockSameStartTime: false,
				MinDayOffsPerWeek: 1,
				MaxDayOffsPerWeek: 3,
				MinConsecutiveWorkdays: 2,
				MaxConsecutiveWorkdays: 6,
				MinConsecutiveDayOffs: 1,
				MaxConsecutiveDayOffs: 3,
				MinFullWeekendsOff: 0,
				MaxFullWeekendsOff: 8,
				MinWeekendDaysOff: 0,
				MaxWeekendDaysOff: 16,
				Priority: -1,
				Id: null,
				Filters: [],
				Default: true,
				Name: $translate.instant('Default')
			}]
		};
		vm.planningGroupSettings = [];
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeSelectedFilter = removeSelectedFilter;
		vm.persist = persist;
		vm.removePlanningGroup = removePlanningGroup;
        vm.goCreateSchedulingSetting = goCreateSchedulingSetting;

		prepareEditInfo();

		function prepareEditInfo() {
			if (!vm.isEditGroup)
				return;
			vm.deletePlanningGroupText = $translate.instant('AreYouSureYouWantToDeleteThePlanningGroup').replace("{0}", editPlanningGroup.Name);
			vm.name = editPlanningGroup.Name;
			vm.planningGroupId = editPlanningGroup.Id;
			vm.selectedResults = editPlanningGroup.Filters.sort(localeLanguageSortingService.localeSort('+FilterType', '+Name'));
		}

		function inputFilterData() {
			return planningGroupService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
				return vm.filterResults = removeSelectedFiltersInList(data, vm.selectedResults);
			});
		}

		function removeSelectedFiltersInList(filters, selectedFilters) {
			var result = angular.copy(filters);
			if (selectedFilters.length === 0 || filters.length === 0)
				return filters;
			for (var i = filters.length - 1; i >= 0; i--) {
				angular.forEach(selectedFilters, function (selectedItem) {
					if (filters[i].Id === selectedItem.Id) {
						result.splice(i, 1);
					}
				});
			}
			return result;
		}

		function selectResultItem(item) {
			if (item == null)
				return;
			if (isValidUnit(item)) {
				vm.selectedResults.push(item);
				vm.selectedResults.sort(localeLanguageSortingService.localeSort('+FilterType', '+Name'));
				clearInput();
			} else {
				clearInput();
				NoticeService.warning("Unit already exists", 5000, true);
			}
		}

		function isValidUnit(item) {
			var check = true;
			vm.selectedResults.forEach(function (node) {
				if (node.Id === item.Id) {
					check = false;
				}
			});
			return check;
		}

		function clearInput() {
			vm.searchString = '';
			vm.results = [];
		}

		function removeSelectedFilter(node) {
			var p = vm.selectedResults.indexOf(node);
			vm.selectedResults.splice(p, 1);
		}

		function isValid() {
			for (var i = 0; i < vm.editPlanningGroup.Settings.length; i++) {
				if(vm.editPlanningGroup.Settings[i].isValid && !vm.editPlanningGroup.Settings[i].isValid()){
                    vm.editPlanningGroup.Settings[i].submitted = true;
					return false;
				}
			}
			if (isValidFilters() && isValidName())
				return true;
		}

		function isValidFilters() {
			return vm.selectedResults.length > 0;
		}

		function isValidName() {
			return vm.name.length > 0 && vm.name.length <= 100;
		}
		
		function returnToPreviousView(){
			if($stateParams.planningPeriodId){
				$state.go('resourceplanner.planningperiodoverview',{
					groupId: $stateParams.groupId,
					ppId: $stateParams.planningPeriodId
				});
			}else {
				returnToOverview();
			}
		}

		function returnToOverview() {
			$state.go('resourceplanner.overview');
		}

		function persist() {
            vm.submitted = true;
			if (!isValid()) {
				NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
			} else if (!vm.requestSent) {
				vm.requestSent = true;
				return planningGroupService.savePlanningGroup({
					Id: vm.isEditGroup ? vm.editPlanningGroup.Id : null,
					Name: vm.name,
					Filters: vm.selectedResults,
					Settings: vm.editPlanningGroup.Settings,
					PreferencePercent: vm.editPlanningGroup.PreferencePercent
				}).$promise.then(function () {
					returnToPreviousView();
				});
			}
		}

		function removePlanningGroup() {
			if (!vm.isEditGroup) return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				return planningGroupService.removePlanningGroup({ id: editPlanningGroup.Id }).$promise.then(function () {
					returnToOverview();
				});
			}
		}

        function goCreateSchedulingSetting() {
			if(isValid()){
                var maxPriority = Math.max.apply(Math, vm.editPlanningGroup.Settings.map(function(item){return item.Priority;}));
                vm.editPlanningGroup.Settings.unshift({
                    BlockFinderType: 0,
                    BlockSameShift: false,
                    BlockSameShiftCategory: false,
                    BlockSameStartTime: false,
                    MinDayOffsPerWeek: 1,
                    MaxDayOffsPerWeek: 3,
                    MinConsecutiveWorkdays: 2,
                    MaxConsecutiveWorkdays: 6,
                    MinConsecutiveDayOffs: 1,
                    MaxConsecutiveDayOffs: 3,
                    MinFullWeekendsOff: 0,
                    MaxFullWeekendsOff: 8,
                    MinWeekendDaysOff: 0,
                    MaxWeekendDaysOff: 16,
                    Priority: maxPriority+1,
                    Id: null,
                    Filters: [],
                    Default: false,
                    Name: "",
                    PlanningGroupId: $stateParams.groupId
                });
			}else{
                NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
                vm.submitted = true;
			}
        }
	}
})();
