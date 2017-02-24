(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('agentGroupFormController', Controller);

	Controller.$inject = ['$state', '$timeout', '$q','agentGroupService', 'NoticeService', '$translate'];

	/* @ngInject */
	function Controller($state, $timeout, $q, agentGroupService, NoticeService, $translate) {
		var vm = this;

		vm.inputFilterData = inputFilterData;
		vm.selectResultItem = selectResultItem;
		vm.isValidFilters = isValidFilters;
		vm.isValidName = isValidName;
		vm.isValid = isValid;
		vm.removeNode = removeNode;
		vm.persist = persist;
		vm.searchString;
		vm.results = [];
		vm.selectedResults = [];
		vm.name = '';
		vm.cancelCreate = cancelCreate;

		function cancelCreate() {
			$state.go('resourceplanner.agentgroups');
		}

		function inputFilterData() {
			var searchString = vm.searchString;

			if (!searchString) {
				return vm.results;
			}
			if (searchString.length > 0) {
        var getFilterData = agentGroupService.getFilterData.query({
          searchString: searchString
        });
        getFilterData.$promise.then(function(data) {
          vm.results = data;
        });
			}
		}

		function selectResultItem(item) {
			if (item === null) {
				return
			}
			if (isVaildUnit(item)) {
				vm.selectedResults.push(item);
				clearInput();
			} else {
				clearInput();
				NoticeService.warning("Unit already exists", 5000, true);
			}
		};

		function removeNode(node) {
			var p = vm.selectedResults.indexOf(node);
			vm.selectedResults.splice(p, 1);
		};

		function clearInput() {
			vm.searchString = '';
			vm.results = [];
		};

		function isVaildUnit(item) {
			var check = true;
			vm.selectedResults.forEach(function(node) {
				if (node.Id === item.Id) {
					check = false
				};
			});
			return check;
		};

		function isValidFilters() {
			return vm.selectedResults.length > 0;
		};

		function isValidName() {
			return vm.name.length > 0 && vm.name.length <= 100;
		};

		function isValid() {
			if (isValidFilters() && isValidName()) {
				return true;
			}
		};

		function persist() {
			if (isValid()) {
				agentGroupService.saveAgentGroup.save({
					Name: vm.name,
					Filters: vm.selectedResults
				}).$promise.then(function() {
					$state.go('resourceplanner.agentgroups');
				});
			}
			if (!isValid()) {
				NoticeService.warning($translate.instant('CouldNotApply'), 5000, true);
				return;
			}
		}
	}
})();
