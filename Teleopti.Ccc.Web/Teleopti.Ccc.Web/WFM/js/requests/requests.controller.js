(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "Toggle", "requestsDefinitions", "requestsNotificationService"];

	function requestsController($scope, toggleService, requestsDefinitions, requestsNotificationService ) {
		var vm = this;
		vm.onAgentSearchTermChanged = onAgentSearchTermChanged;
		
		toggleService.togglesLoaded.then(init);
		
		function init() {
			vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
			vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
			vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.isShiftTradeViewVisible = toggleService.Wfm_Requests_ShiftTrade_37751;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
			vm.isShiftTradeViewActive = isShiftTradeViewActive;
			
			vm.dateRangeTemplateType = 'popup';
			
			vm.filterToggleEnabled = toggleService.Wfm_Requests_Filtering_37748;
			vm.filterEnabled = vm.filterToggleEnabled;
			
			vm.period = { startDate: new Date(), endDate: new Date() };
			
					
			vm.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false
			};
			vm.agentSearchTerm = vm.agentSearchOptions.keyword;

			vm.onBeforeCommand = onBeforeCommand;
			vm.onCommandSuccess = onCommandSuccess;
			vm.onCommandError = onCommandError;
			vm.onErrorMessages = onErrorMessages;
			vm.disableInteraction = false;

		}

		function isShiftTradeViewActive() {
			return vm.selectedTabIndex === 1;
		}

		
		function onAgentSearchTermChanged(agentSearchTerm) {
			vm.agentSearchTerm = agentSearchTerm;
		}

		function forceRequestsReloadWithoutSelection() {
			$scope.$broadcast('reload.requests.without.selection');
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
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Cancel) {
				requestsNotificationService.notifyCancelledRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist) {
			    requestsNotificationService.notifySubmitProcessWaitlistedRequestsSuccess();
			}
		}

		function onErrorMessages(errorMessages) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();

			errorMessages.forEach(function(errorMessage) {
				requestsNotificationService.notifyCommandError(errorMessage);
			});
		}
		//Todo: submit command failure doesn't give an error info, this parameter will be undefined.
		function onCommandError(error) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyCommandError(error);
		}

		
	}

})();