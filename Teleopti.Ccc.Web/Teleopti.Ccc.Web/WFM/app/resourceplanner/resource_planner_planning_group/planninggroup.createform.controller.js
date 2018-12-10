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
		vm.cancel = returnToOverview;
		vm.deletePlanningGroupText = '';
		vm.editPlanningGroup = editPlanningGroup;
		vm.planningGroupId = null;
		vm.isEditGroup = !!editPlanningGroup;
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeSelectedFilter = removeSelectedFilter;
		vm.persist = persist;
		vm.removePlanningGroup = removePlanningGroup;

		prepareEditInfo();

		function prepareEditInfo() {
			if (editPlanningGroup == null)
				return;
			vm.deletePlanningGroupText = $translate.instant('AreYouSureYouWantToDeleteThePlanningGroup').replace("{0}", editPlanningGroup.Name);
			vm.name = editPlanningGroup.Name;
			vm.planningGroupId = editPlanningGroup.Id;
			vm.selectedResults = editPlanningGroup.Filters.sort(localeLanguageSortingService.localeSort('+FilterType', '+Name'));
		}

		function inputFilterData() {
			if (vm.searchString === '')
				return [];
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
			if (isValidFilters() && isValidName())
				return true;
		}

		function isValidFilters() {
			return vm.selectedResults.length > 0;
		}

		function isValidName() {
			return vm.name.length > 0 && vm.name.length <= 100;
		}

		function returnToOverview() {
			$state.go('resourceplanner.overview');
		}

		function persist() {
			if (!isValid()) {
				NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
			} else if (!vm.requestSent) {
				vm.requestSent = true;
				return planningGroupService.savePlanningGroup({
					Id: editPlanningGroup ? editPlanningGroup.Id : null,
					Name: vm.name,
					Filters: vm.selectedResults,
					Settings: editPlanningGroup ? editPlanningGroup.Settings : [],
					PreferencePercent: editPlanningGroup ? editPlanningGroup.PreferencePercent : 0
				}).$promise.then(function () {
					returnToOverview();
				});
			}
		}

		function removePlanningGroup() {
			if (!editPlanningGroup) return;
			if (!vm.requestSent) {
				vm.requestSent = true;
				return planningGroupService.removePlanningGroup({ id: editPlanningGroup.Id }).$promise.then(function () {
					returnToOverview();
				});
			}
		}
	}
})();
