(function() {
	'use strict';

	angular.module('wfm.requests')
		.controller('requestsCommandsPaneCtrl', requestsCommandsPaneCtrl)
		.directive('requestsCommandsPane', requestsCommandsPaneDirective);


	requestsCommandsPaneCtrl.$inject = [
		'requestsDefinitions', 'requestsDataService', 'requestCommandParamsHolder', 'Toggle',
		'signalRSVC'
	];

	function requestsCommandsPaneCtrl(requestsDefinitions, requestsDataService, requestCommandParamsHolder, toggleSvc,
		signalRSVC) {
		var vm = this;

		vm.approveRequests = approveRequests;
		vm.denyRequests = denyRequests;
		vm.disableCommands = disableCommands;
		vm.canCancelRequests = canCancelRequests;
		vm.cancelToggleIsEnabled = cancelToggleIsEnabled;
		vm.toggleCancelConfirmationModal = toggleCancelConfirmationModal;
		vm.toggleProcessWaitlistModal = toggleProcessWaitlistModal;
		vm.processWaitlistRequests = processWaitlistRequests;
		vm.runWaitlistToggleIsEnabled = runWaitlistToggleIsEnabled;
		vm.cancelRequests = cancelRequests;
		vm.approveBasedOnBudget = approveBasedOnBudget;
		vm.isApproveBasedOnBudgetEnabled = isApproveBasedOnBudgetEnabled;
		initWaitlistProcessPeriod();
		subscribeSignalRMessage('IRunRequestWaitlistEventMessage', vm.onProcessWaitlistFinished);
		subscribeSignalRMessage('IApproveRequestsWithValidatorsEventMessage', vm.onApproveBasedOnBudgetFinished);

		function handleErrorMessages(errorMessages) {
			if (vm.onErrorMessages) {
				vm.onErrorMessages({ errorMessages: errorMessages });
			}
		}

		function initWaitlistProcessPeriod() {
			vm.waitlistPeriod = { startDate: new Date(), endDate: new Date() };
		}

		function getSelectedRequestIds() {
			return requestCommandParamsHolder
				? requestCommandParamsHolder.getSelectedRequestsIds(vm.isShiftTradeViewActive)
				: null;
		}

		function doProcessWaitlistCommandHandling(waitlistPeriod) {
			var requestType = requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist;
			var dataServicePromise = requestsDataService.processWaitlistRequestsPromise;
			var commandInProgress = dataServicePromise(waitlistPeriod);

			if (vm.afterCommandSuccess) {
				commandInProgress.success(function(requestCommandHandlingResult) {
					if (requestCommandHandlingResult.Success || (requestCommandHandlingResult.AffectedRequestIds
						&& requestCommandHandlingResult.AffectedRequestIds.length > 0)) {
						vm.commandTrackId = requestCommandHandlingResult.CommandTrackId;
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: null,
							waitlistPeriod: waitlistPeriod
						});
					}
					if (requestCommandHandlingResult.ErrorMessages && requestCommandHandlingResult.ErrorMessages.length > 0) {
						handleErrorMessages(requestCommandHandlingResult.ErrorMessages);
					}
				});
			}
			if (vm.afterCommandError) {
				commandInProgress.error(vm.afterCommandError);
			}
		}

		function doStandardCommandHandlingWithParameters(requestType, dataServicePromise, parameters) {
			if (vm.beforeCommand && !vm.beforeCommand()) return;
			var commandInProgress = parameters === undefined
				? dataServicePromise()
				: dataServicePromise(parameters);

			if (vm.afterCommandSuccess) {
				var requestCount = 0;
				if (parameters !== undefined && parameters !== null) {
					if (Array.isArray(parameters)) {
						requestCount = parameters.length;
					} else if (Array.isArray(parameters.selectedRequestIds)) {
						requestCount = parameters.selectedRequestIds.length;
					}
				}

				commandInProgress.success(function(requestCommandHandlingResult) {
					if (requestCommandHandlingResult.Success || (requestCommandHandlingResult.AffectedRequestIds
						&& requestCommandHandlingResult.AffectedRequestIds.length > 0)) {
						vm.commandTrackId = requestCommandHandlingResult.CommandTrackId;
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: requestCount
						});
					}
					if (requestCommandHandlingResult.ErrorMessages && requestCommandHandlingResult.ErrorMessages.length > 0) {
						handleErrorMessages(requestCommandHandlingResult.ErrorMessages);
					}
				});
			}
			if (vm.afterCommandError) {
				commandInProgress.error(vm.afterCommandError);
			}
		}

		function doStandardCommandHandling(requestType, dataServicePromise) {
			var selectedRequestIds = getSelectedRequestIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;
			doStandardCommandHandlingWithParameters(requestType, dataServicePromise, selectedRequestIds);
		}

		function subscribeSignalRMessage(domainType, eventHandler) {
			signalRSVC.subscribe({ DomainType: domainType }, function (message) {
				if (message.TrackId == vm.commandTrackId) {
					eventHandler({ message: message });
				}
			});
		}

		function approveRequests() {
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Approve, requestsDataService.approveRequestsPromise);
		}

		function processWaitlistRequests() {
			vm.toggleProcessWaitlistModal();
			doProcessWaitlistCommandHandling(vm.waitlistPeriod);
			initWaitlistProcessPeriod();
		}

		function approveBasedOnBudget() {
			var selectedRequestIds = getSelectedRequestIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;

			var parameter = {
				RequestIds: selectedRequestIds,
				Validators: requestsDefinitions.REQUEST_VALIDATORS.BudgetAllotmentValidator
			};

			doStandardCommandHandlingWithParameters(requestsDefinitions.REQUEST_COMMANDS.ApproveBasedOnBudget,
				requestsDataService.approveWithValidatorsPromise, parameter);
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
		
		function cancelRequests() {
			vm.toggleCancelConfirmationModal();
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Cancel, requestsDataService.cancelRequestsPromise);
		}

		function toggleCancelConfirmationModal() {
			if (canCancelRequests()) {
				vm.ShowCancelAbsenceConfirmationModal = !vm.ShowCancelAbsenceConfirmationModal;
			}
		}

		function toggleProcessWaitlistModal() {
			vm.showProcessWaitlistModal = !vm.showProcessWaitlistModal;
		}

		function isApproveBasedOnBudgetEnabled() {
			return toggleSvc.Wfm_Requests_Approve_Based_On_Budget_Allotment_39626 && !vm.isShiftTradeViewActive;
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
				isShiftTradeViewActive: '=',
				onProcessWaitlistFinished: '&?',
				onApproveBasedOnBudgetFinished: '&?'
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-commands-pane.tpl.html'
		};
	}
})();