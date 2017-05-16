(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$stateParams', 'agentGroupService', 'NoticeService', '$translate', 'debounceService'];

	function Controller($state, $timeout, $stateParams, agentGroupService, NoticeService, $translate, debounceService) {
		var vm = this;

		var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;
		vm.searchString = '';
		vm.selectedResults = [];
		vm.filterResults = [];
		vm.name = '';
		vm.cancel = cancel;
		vm.editAgentGroup = {};
		vm.deleteAgentGroupText = '';
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeSelectedFilter = removeSelectedFilter;
		vm.persist = persist;
		vm.removeAgentGroup = removeAgentGroup;

		getAgentGroupById();

		function getAgentGroupById() {
			if (agentGroupId == null)
				return;
			var getAgentGroup = agentGroupService.getAgentGroupById({ id: agentGroupId });
			return getAgentGroup.$promise.then(function (data) {
				vm.editAgentGroup = data;
				vm.deleteAgentGroupText = $translate.instant("AreYouSureYouWantToDeleteTheAgentGroup").replace("{0}", vm.editAgentGroup.Name);
				vm.name = data.Name;
				vm.selectedResults = data.Filters;
				return vm.editAgentGroup;
			});
		}

		function inputFilterData() {
			if (vm.searchString == '')
				return [];
			var filters = agentGroupService.getFilterData({ searchString: vm.searchString });
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
			agentGroupService.saveAgentGroup({
				Id: vm.editAgentGroup.Id,
				Name: vm.name,
				Filters: vm.selectedResults
			}).$promise.then(function () {
				vm.editAgentGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}

		function cancel() {
			$state.go('resourceplanner.newoverview');
			vm.editAgentGroup = {};
		}

		function removeAgentGroup(id) {
			if (!id) return;
			agentGroupService.removeAgentGroup({ id: id }).$promise.then(function () {
				vm.editAgentGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}
	}
})();
