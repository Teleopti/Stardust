(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$stateParams', 'planningGroupService', 'NoticeService', '$translate', 'debounceService', 'localeLanguageSortingService', 'editPlanningGroup'];

	function Controller($state, $timeout, $stateParams, planningGroupService, NoticeService, $translate, debounceService, localeLanguageSortingService, editPlanningGroup) {
		var vm = this;

		vm.searchString = '';
		vm.selectedResults = [];
		vm.filterResults = [];
		vm.name = '';
		vm.cancel = cancel;
		vm.deletePlanningGroupText = '';
		vm.editPlanningGroup = editPlanningGroup;
		vm.isEditGroup = editPlanningGroup ? true : false;
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
			vm.deletePlanningGroupText = $translate.instant("AreYouSureYouWantToDeleteThePlanningGroup").replace("{0}", editPlanningGroup.Name);
			vm.name = editPlanningGroup.Name;
			vm.selectedResults = editPlanningGroup.Filters.sort(localeLanguageSortingService.localeSort('+FilterType', '+Name'));
		}

		function inputFilterData() {
			if (vm.searchString == '')
				return [];
			var filters = planningGroupService.getFilterData({ searchString: vm.searchString });
			return filters.$promise.then(function (data) {
				removeSelectedFiltersInList(data, vm.selectedResults);
				return vm.filterResults = data;
			});
		}

		function removeSelectedFiltersInList(filters, selectedFilters) {
			if (selectedFilters.length == 0)
				return;
			for (var i = filters.length - 1; i >= 0; i--) {
				angular.forEach(selectedFilters, function (selectedItem) {
					if (filters[i].Id === selectedItem.Id) {
						filters.splice(i, 1);
					}
				});
			}
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
				};
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

		function persist() {
			if (!isValid()) {
				NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
				return;
			}
			return planningGroupService.savePlanningGroup({
				Id: editPlanningGroup ? editPlanningGroup.Id : null,
				Name: vm.name,
				Filters: vm.selectedResults
			}).$promise.then(function () {
				$state.go('resourceplanner.newoverview');
			});
		}

		function cancel() {
			$state.go('resourceplanner.newoverview');
		}

		function removePlanningGroup() {
			if (!editPlanningGroup) return;
			return planningGroupService.removePlanningGroup({ id: editPlanningGroup.Id }).$promise.then(function () {
				$state.go('resourceplanner.newoverview');
			});
		}
	}
})();
