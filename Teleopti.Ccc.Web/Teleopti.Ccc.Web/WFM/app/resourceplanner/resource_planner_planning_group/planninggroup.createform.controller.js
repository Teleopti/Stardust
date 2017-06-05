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
		vm.editPlanGroup = {};
		vm.deletePlanGroupText = '';
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeSelectedFilter = removeSelectedFilter;
		vm.persist = persist;
		vm.removePlanGroup = removePlanGroup;

		getPlanGroupById();

		function getPlanGroupById() {
			if (planningGroupId == null)
				return;
			var getPlanGroup = planningGroupService.getPlanGroupById({ id: planningGroupId });
			return getPlanGroup.$promise.then(function (data) {
				vm.editPlanGroup = data;
				vm.deletePlanGroupText = $translate.instant("AreYouSureYouWantToDeleteThePlanGroup").replace("{0}", vm.editPlanGroup.Name);
				vm.name = data.Name;
				vm.selectedResults = data.Filters.sort(localeLanguageSortingService.localeSort('+FilterType','+Name'));
				return vm.editPlanGroup;
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
			planningGroupService.savePlanGroup({
				Id: vm.editPlanGroup.Id,
				Name: vm.name,
				Filters: vm.selectedResults
			}).$promise.then(function () {
				vm.editPlanGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}

		function cancel() {
			$state.go('resourceplanner.newoverview');
			vm.editPlanGroup = {};
		}

		function removePlanGroup(id) {
			if (!id) return;
			planningGroupService.removePlanGroup({ id: id }).$promise.then(function () {
				vm.editPlanGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}
	}
})();
