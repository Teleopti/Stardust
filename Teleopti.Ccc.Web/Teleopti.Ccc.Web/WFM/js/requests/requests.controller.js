(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "Toggle", "requestsDefinitions", "requestsNotificationService", "CurrentUserInfo", "signalRSVC"];

    function requestsController($scope, toggleService, requestsDefinitions, requestsNotificationService, CurrentUserInfo, signalRSVC) {
        var vm = this;
        vm.onAgentSearchTermChanged = onAgentSearchTermChanged;

        toggleService.togglesLoaded.then(init);
		
        function init() {
            monitorRunRequestWaitlist();
            vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
            vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
            vm.isShiftTradeViewActive = isShiftTradeViewActive;
            vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.isShiftTradeViewVisible = toggleService.Wfm_Requests_ShiftTrade_37751;
            vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
		
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

        function onCommandSuccess(commandType, changedRequestsCount, requestsCount, commandId, waitlistPeriod) {
            vm.disableInteraction = false;
            forceRequestsReloadWithoutSelection();
            if (commandId) vm.commandIdForMessage = commandId;
            if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
                requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
                requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Cancel) {
                requestsNotificationService.notifyCancelledRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist) {
                var period = moment(waitlistPeriod.startDate).format("L") + "-" + moment(waitlistPeriod.endDate).format("L");
                requestsNotificationService.notifySubmitProcessWaitlistedRequestsSuccess(period);
            }
        }

        function monitorRunRequestWaitlist() {
            signalRSVC.subscribe(
				{ DomainType: 'IRunRequestWaitlistEventMessage' }
				, RunRequestWaitlistEventHandler);
        }

        function formatDatePeriod(message) {
            vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
            var startDate = moment(message.StartDate.substring(1, message.StartDate.length)).tz(vm.userTimeZone).format("L");
            var endDate = moment(message.EndDate.substring(1, message.EndDate.length)).tz(vm.userTimeZone).format("L");
            return startDate + "-" + endDate;
        }

        function RunRequestWaitlistEventHandler(message) {
            if (vm.commandIdForMessage === message.TrackId) {
                var period = formatDatePeriod(message);
                requestsNotificationService.notifyProcessWaitlistedRequestsFinished(period);
            }
        }

        function onErrorMessages(errorMessages) {
            vm.disableInteraction = false;
            forceRequestsReloadWithoutSelection();

            errorMessages.forEach(function (errorMessage) {
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