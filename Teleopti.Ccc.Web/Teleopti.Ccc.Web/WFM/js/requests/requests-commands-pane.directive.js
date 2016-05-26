(function () {
	'use strict';

	angular.module('wfm.requests')
		.controller('requestsCommandsPaneCtrl', requestsCommandsPaneCtrl)
		.directive('requestsCommandsPane', requestsCommandsPaneDirective)


	requestsCommandsPaneCtrl.$inject = ['requestsDefinitions', 'requestsDataService', 'requestCommandParamsHolder','Toggle'];

	function requestsCommandsPaneCtrl(requestsDefinitions, requestsDataService, requestCommandParamsHolder, toggleSvc) {
		var vm = this;

		vm.approveRequests = approveRequests;
		vm.denyRequests = denyRequests;
		vm.disableCommands = disableCommands;
		vm.canCancelRequests = canCancelRequests;
		vm.cancelRequests = cancelRequests;
		vm.cancelToggleIsEnabled = cancelToggleIsEnabled;
		vm.toggleCancelConfirmationModal = toggleCancelConfirmationModal;
		vm.toggleProcessWaitlistModal = toggleProcessWaitlistModal;
		vm.processWaitlistRequests = processWaitlistRequests;
	    vm.runWaitlistToggleIsEnabled = runWaitlistToggleIsEnabled;
		initWaitlistProcessPeriod();

		function handleErrorMessages(errorMessages) {
			if (vm.onErrorMessages) {
				vm.onErrorMessages({ errorMessages: errorMessages });
			}
			
		}

		function initWaitlistProcessPeriod() {
			vm.waitlistPeriod = { startDate: new Date(), endDate: new Date() };
		}

		function getSelectedRequestIds() {
			
			return requestCommandParamsHolder ? requestCommandParamsHolder.getSelectedRequestsIds(vm.isShiftTradeViewActive) : null;
		}

		function doStandardCommandHandling(requestType, dataServicePromise,useStraight) {

			if (!useStraight) {
				var selectedRequestIds = getSelectedRequestIds();
				if (!selectedRequestIds || selectedRequestIds.length === 0) return;
				if (vm.beforeCommand && !vm.beforeCommand()) return;
				var commandInProgress = dataServicePromise(selectedRequestIds);
			} else {
				var commandInProgress = dataServicePromise(vm.waitlistPeriod);
			}
			
			
			if (vm.afterCommandSuccess) {
				commandInProgress.success(function (requestCommandHandlingResult) {

					if (requestCommandHandlingResult.Success) {
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: useStraight == true ? null : selectedRequestIds.length
					});
					} else {
						handleErrorMessages(requestCommandHandlingResult.ErrorMessages);
					}
				});
			}
			if (vm.afterCommandError) {
				commandInProgress.error(vm.afterCommandError);
			}

		}

		function approveRequests() {
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Approve, requestsDataService.approveRequestsPromise);
		}

		function processWaitlistRequests() {
		    vm.toggleProcessWaitlistModal();
		    doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist, requestsDataService.processWaitlistRequestsPromise,true);
			initWaitlistProcessPeriod();
		}

		function cancelRequests() {
			vm.toggleCancelConfirmationModal();
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Cancel, requestsDataService.cancelRequestsPromise);
		}

		function denyRequests() {
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Deny, requestsDataService.denyRequestsPromise);
		}

		function disableCommands() {
			var selectedRequestIds = getSelectedRequestIds();
			if (vm.commandsDisabled) return true;
			return !selectedRequestIds || selectedRequestIds.length === 0;
		}

		function cancelToggleIsEnabled() {
			return toggleSvc.Wfm_Requests_Cancel_37741 === true;
		}

        function runWaitlistToggleIsEnabled() {
            return toggleSvc.Wfm_Requests_Run_waitlist_38071 === true;
        }

		function canCancelRequests() {
			return !disableCommands();
		}

		function toggleCancelConfirmationModal() {
			vm.ShowCancelAbsenceConfirmationModal = !vm.ShowCancelAbsenceConfirmationModal;
		}

        function toggleProcessWaitlistModal() {
            vm.showProcessWaitlistModal = !vm.showProcessWaitlistModal;
        }
	}

	function requestsCommandsPaneDirective() {
		return {
			controller: 'requestsCommandsPaneCtrl',
			controllerAs: 'requestsCommandsPane',
			bindToController: true,
			scope: {
				beforeCommand: '&?',
				afterCommandSuccess: '&?',
				afterCommandError: '&?',
				onErrorMessages: '&?',
				commandsDisabled: '=?',
				isShiftTradeViewActive: '='
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-commands-pane.tpl.html'
		};
	}


})();