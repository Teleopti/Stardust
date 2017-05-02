(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$stateParams', 'agentGroupService', 'NoticeService', '$translate', 'debounceService'];

	function Controller($state, $timeout, $stateParams, agentGroupService, NoticeService, $translate, debounceService) {
		var vm = this;

		vm.searchString = '';
		vm.selectedResults = [];
		vm.filterResults = [];
		vm.name = '';
		vm.cancel = cancel;
		vm.editAgentGroup = {};
		vm.inputFilterData = debounceService.debounce(inputFilterData, 250);
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeNode = removeNode;
		vm.persist = persist;
		vm.removeAgentGroup = removeAgentGroup;
		var agentGroupId = $stateParams.groupId ? $stateParams.groupId : null;

		getAgentGroupbyId(agentGroupId);

		function getAgentGroupbyId(id) {
			if (id !== null) {
				var getAgentGroup = agentGroupService.getAgentGroupbyId({ id: id });
				return getAgentGroup.$promise.then(function (data) {
					vm.editAgentGroup = data;
					vm.name = data.Name;
					vm.selectedResults = data.Filters;
					return vm.editAgentGroup;
				});
			}
		}

		function cancel() {
			$state.go('resourceplanner.newoverview');
			vm.editAgentGroup = {};
		}

		function inputFilterData() {
			if (vm.searchString !== '') {
				var filters = agentGroupService.getFilterData({ searchString: vm.searchString }).$promise.then(function (data) {
					removeSelectedFiltersInList(data, vm.selectedResults)
					return vm.filterResults = data;
				});
				return filters;
			} else {
				return [];
			}
		}

		function removeSelectedFiltersInList(filters, selectedFilters) {
			if (selectedFilters.length > 0){
				for (var i = filters.length - 1; i >= 0; i--) {
					angular.forEach(selectedFilters, function (selectedItem) {
						if (filters[i].Id === selectedItem.Id) {
							filters.splice(i, 1);
						}
					});
				}
			}
		}

		function selectResultItem(item) {
			if (item == null) {
				return;
			}
			if (isVaildUnit(item)) {
				vm.selectedResults.push(item);
				clearInput();
			} else {
				clearInput();
				NoticeService.warning("Unit already exists", 5000, true);
			}
		}

		function isVaildUnit(item) {
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

		function removeNode(node) {
			var p = vm.selectedResults.indexOf(node);
			vm.selectedResults.splice(p, 1);
		}

		function isValid() {
			if (isValidFilters() && isValidName()) {
				return true;
			}
		}

		function isValidFilters() {
			return vm.selectedResults.length > 0;
		}

		function isValidName() {
			return vm.name.length > 0 && vm.name.length <= 100;
		}

		function persist() {
			var id = vm.editAgentGroup ? vm.editAgentGroup.Id : null;
			if (isValid()) {
				agentGroupService.saveAgentGroup({
					Id: id,
					Name: vm.name,
					Filters: vm.selectedResults
				}).$promise.then(function () {
					vm.editAgentGroup = {};
					$state.go('resourceplanner.newoverview');
				});
			}
			if (!isValid()) {
				NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
				return;
			}
		}

		function removeAgentGroup(id) {
			agentGroupService.removeAgentGroup({ id: id }).$promise.then(function () {
				vm.editAgentGroup = {};
				$state.go('resourceplanner.newoverview');
			});
		}
	}
})();
