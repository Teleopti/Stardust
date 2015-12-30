(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "RequestsToggles", "requestsDefinitions", "requestsNotificationService"];

	function requestsController($scope, requestsToggles, requestsDefinitions, requestsNotificationService) {
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
			vm.isRequestsCommandsEnabled = toggles.isRequestsCommandsEnabled();

			vm.period = { startDate: new Date(), endDate: new Date() };
			vm.paging = {
				pageSize: 10,
				pageNumber: 1,
				totalPages: 1,
				totalRequestsCount: 0
			};
					
			vm.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false
			};
			vm.agentSearchTerm = vm.agentSearchOptions.keyword;

			vm.onBeforeCommand = onBeforeCommand;
			vm.onCommandSuccess = onCommandSuccess;
			vm.onCommandError = onCommandError;
			vm.disableInteraction = false;
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

		function forceRequestsReload() {
			$scope.$broadcast('reload.requests.immediately');
		}

		function onBeforeCommand() {
			vm.disableInteraction = true;			
			return true;
		}

		function onCommandSuccess(commandType, changedRequestsCount) {
			vm.disableInteraction = false;
			forceRequestsReload();

			if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
				requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
				requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount);
			}
		}

		function onCommandError(error) {
			vm.disableInteraction = false;
			forceRequestsReload();
			requestsNotificationService.notifyCommandError(error);
		}

		
	}

})();