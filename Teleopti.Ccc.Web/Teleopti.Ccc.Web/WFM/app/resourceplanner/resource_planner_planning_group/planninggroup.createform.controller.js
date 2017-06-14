(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$stateParams', 'planningGroupService', 'NoticeService', '$translate', 'debounceService', 'localeLanguageSortingService'];

	function Controller($state, $timeout, $stateParams, planningGroupService, NoticeService, $translate, debounceService, localeLanguageSortingService) {
		var vm = this;

		var planningGroupId = $stateParams.groupId ? $stateParams.groupId : null;
		vm.searchString = '';
		vm.selectedResults = [];
		vm.filterResults = [];
		vm.name = '';
		vm.cancel = cancel;
		vm.editPlanningGroup = {};
		vm.deletePlanningGroupText = '';
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeSelectedFilter = removeSelectedFilter;
		vm.persist = persist;
		vm.removePlanningGroup = removePlanningGroup;

		getPlanningGroupById();

		function getPlanningGroupById() {
			if (planningGroupId == null)
				return;
			var getPlanningGroup = planningGroupService.getPlanningGroupById({ id: planningGroupId });
			return getPlanningGroup.$promise.then(function (data) {
				vm.editPlanningGroup = data;
				vm.deletePlanningGroupText = $translate.instant("AreYouSureYouWantToDeleteThePlanningGroup").replace("{0}", vm.editPlanningGroup.Name);
				vm.name = data.Name;
				vm.selectedResults = data.Filters.sort(localeLanguageSortingService.localeSort('+FilterType','+Name'));
				return vm.editPlanningGroup;
			});
		}

		function inputFilterData() {
			if (vm.searchString == '')
				return [];
			var filters = planningGroupService.getFilterData({ searchString: vm.searchString });
			filters.$promise.then(function (data) {
				removeSelectedFiltersInList(data, vm.selectedResults);
				return vm.filterResults = data;
			});
			return filters;
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
				vm.selectedResults.sort(localeLanguageSortingService.localeSort('+FilterType','+Name'));
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
			planningGroupService.savePlanningGroup({
				Id: vm.editPlanningGroup.Id,
				Name: vm.name,
				Filters: vm.selectedResults
			}).$promise.then(function () {
				vm.editPlanningGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}

		function cancel() {
			$state.go('resourceplanner.newoverview');
			vm.editPlanningGroup = {};
		}

		function removePlanningGroup(id) {
			if (!id) return;
			planningGroupService.removePlanningGroup({ id: id }).$promise.then(function () {
				vm.editPlanningGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}
	}
})();
