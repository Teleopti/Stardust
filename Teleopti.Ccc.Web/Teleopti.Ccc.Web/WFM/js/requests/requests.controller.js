(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestCommandParamsHolder", "$translate"];

	function requestsController($scope, toggleService, requestsDefinitions, requestsNotificationService, requestCommandParamsHolder, $translate) {
		var vm = this;
		vm.onAgentSearchTermChanged = onAgentSearchTermChanged;
		vm.onTotalRequestsCountChanges = onTotalRequestsCountChanges;
		vm.onPageSizeChanges = onPageSizeChanges;
		vm.pageSizeOptions = [20, 50, 100, 200];

		toggleService.togglesLoaded.then(init);
				
		function init() {
			vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
			vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
			vm.isPaginationEnabled = toggleService.Wfm_Requests_Performance_36295;
			vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
			vm.forceRequestsReloadWithSelection = forceRequestsReloadWithSelection;
			getSelectedRequestsInfoText();
			vm.showSelectedRequestsInfo = showSelectedRequestsInfo;
			

			vm.period = { startDate: new Date(), endDate: new Date() };
			vm.paging = {
				pageSize: 50,
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

		function getSelectedRequestsInfoText() {
			$translate("SelectedRequestsInfo").then(function (text) {
				vm.selectedRequestsInfoText = text;
			});
		}

		function showSelectedRequestsInfo() {
			vm.selectedRequestsCount = requestCommandParamsHolder.getSelectedRequestsIds().length;
			if (vm.selectedRequestsCount > 0 && vm.selectedRequestsInfoText) {
				return vm.selectedRequestsInfoText.replace(/\{0\}|\{1\}/gi, function(target) {
					if (target == '{0}') return vm.selectedRequestsCount;
					if (target == '{1}') return vm.paging.totalRequestsCount;
				});
			} else {
				return '';
			}
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
			forceRequestsReloadWithSelection();
		}

		function forceRequestsReloadWithoutSelection() {
			$scope.$broadcast('reload.requests.without.selection');
		}

		function forceRequestsReloadWithSelection() {
			$scope.$broadcast('reload.requests.with.selection');
		}

		function onBeforeCommand() {
			vm.disableInteraction = true;			
			return true;
		}

		function onCommandSuccess(commandType, changedRequestsCount, requestsCount) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();

			if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
				requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
				requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount, requestsCount);
			}
		}

		function onCommandError(error) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyCommandError(error);
		}

		
	}

})();