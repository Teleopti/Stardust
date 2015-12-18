(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["RequestsToggles"];

	function requestsController(requestsToggles) {
		var vm = this;
		vm.onAgentSearchTermChanged = onAgentSearchTermChanged;
		vm.onTotalRequestsCountChanges = onTotalRequestsCountChanges;
		vm.onPageSizeChanges = onPageSizeChanges;
		vm.pageSizeOptions = [10, 20, 50, 100, 200];

		requestsToggles.togglePromise.then(init);

		function init(toggles) {
			vm.isRequestsEnabled = toggles.isRequestsEnabled();			
			vm.isPeopleSearchEnabled = toggles.isPeopleSearchEnabled();
			vm.isPaginationEnabled = toggles.isPaginationEnabled();
			vm.period = { startDate: new Date(), endDate: new Date() };

			vm.paging = {
				pageSize: 10,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 0
			};

			vm.templateType = "dropdown";
			vm.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false
			};
			vm.agentSearchTerm = vm.agentSearchOptions.keyword;
		}

		function onAgentSearchTermChanged(agentSearchTerm) {
			vm.agentSearchTerm = agentSearchTerm;
		}

		function onTotalRequestsCountChanges(totalRequestsCount) {
			var totalPages = Math.ceil(totalRequestsCount / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;
			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = totalRequestsCount;			
		}

		function onPageSizeChanges() {			
			vm.paging.totalPages = Math.ceil(vm.paging.totalRequestsCount / vm.paging.pageSize);
			vm.paging.pageNumber = 1;			
		}

		
	}

})();