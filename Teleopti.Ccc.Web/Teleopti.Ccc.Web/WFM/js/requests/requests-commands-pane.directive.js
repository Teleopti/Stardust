(function () {
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
		vm.replyRequests = replyRequests;
		vm.displayReplyDialog = displayReplyDialog;
		vm.isRequestsReplyMessageEnabled = isRequestsReplyMessageEnabled;
		vm.denyRequests = denyRequests;
		vm.disableCommands = disableCommands;
		vm.canCancelRequests = canCancelRequests;
		vm.cancelToggleIsEnabled = cancelToggleIsEnabled;
		vm.toggleCancelConfirmationModal = toggleCancelConfirmationModal;
		vm.toggleProcessWaitlistModal = toggleProcessWaitlistModal;
		vm.processWaitlistRequests = processWaitlistRequests;
		vm.runWaitlistToggleIsEnabled = runWaitlistToggleIsEnabled;
		vm.cancelRequests = cancelRequests;
		vm.approveBasedOnBusinessRules = approveBasedOnBusinessRules;
		vm.isApproveBasedOnBusinessRulesEnabled = isApproveBasedOnBusinessRulesEnabled;
		initWaitlistProcessPeriod();
		subscribeSignalRMessage('IRunRequestWaitlistEventMessage', vm.onProcessWaitlistFinished);
		subscribeSignalRMessage('IApproveRequestsWithValidatorsEventMessage', vm.onApproveBasedOnBusinessRulesFinished);

		function handleErrorMessages(errorMessages) {
			if (vm.onErrorMessages) {
				vm.onErrorMessages({ errorMessages: errorMessages });
			}
		}

		function initWaitlistProcessPeriod() {
			vm.waitlistPeriod = { startDate: new Date(), endDate: new Date() };
		}

		function getSelectedRequestMessage() {
			var selectedRequestIds = getSelectedRequestIds();
			if (selectedRequestIds.length === 1) {
				vm.selectedRequestMessage = requestCommandParamsHolder.getSelectedIdAndMessage(selectedRequestIds[0]);
			} else {
				vm.selectedRequestMessage = '';
			}
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
				commandInProgress.success(function (requestCommandHandlingResult) {
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
					} else if (Array.isArray(parameters.SelectedRequestIds)) {
						requestCount = parameters.SelectedRequestIds.length;
					}
				}

				commandInProgress.success(function (requestCommandHandlingResult) {
					if (requestCommandHandlingResult.Success || (requestCommandHandlingResult.AffectedRequestIds
						&& requestCommandHandlingResult.AffectedRequestIds.length > 0)) {
						vm.commandTrackId = requestCommandHandlingResult.CommandTrackId;
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: requestCount
						});
					}
					if (requestCommandHandlingResult.ReplySuccess && requestCommandHandlingResult.ReplySuccess.length > 0) {
						vm.afterCommandSuccess({
							commandType: requestsDefinitions.REQUEST_COMMANDS.Reply,
							changedRequestsCount: requestCommandHandlingResult.ReplySuccess.length
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

		function doStandardCommandHandling(requestType, dataServicePromise, replyMessage) {
			var selectedRequestIds = getSelectedRequestIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;
			var selectedRequestIdsAndMessage = {
				ReplyMessage: replyMessage,
				SelectedRequestIds: selectedRequestIds
			}
			doStandardCommandHandlingWithParameters(requestType, dataServicePromise, selectedRequestIdsAndMessage);
		}

		function subscribeSignalRMessage(domainType, eventHandler) {
			signalRSVC.subscribe({ DomainType: domainType }, function (message) {
				if (message.TrackId == vm.commandTrackId) {
					eventHandler({ message: message });
				}
			});
		}

		function replyRequests(message) {
			doStandardCommandHandling(null, requestsDataService.replyRequestsPromise, message);
		}

		function approveRequests(replyMessage) {
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Approve, requestsDataService.approveRequestsPromise, replyMessage);
		}

		function processWaitlistRequests() {
			vm.toggleProcessWaitlistModal();
			doProcessWaitlistCommandHandling(vm.waitlistPeriod);
			initWaitlistProcessPeriod();
		}

		function approveBasedOnBusinessRules() {
			var selectedRequestIds = getSelectedRequestIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;

			var parameter = {
				RequestIds: selectedRequestIds,
				Validators: requestsDefinitions.REQUEST_VALIDATORS.BudgetAllotmentValidator
			};

			doStandardCommandHandlingWithParameters(requestsDefinitions.REQUEST_COMMANDS.ApproveBasedOnBusinessRules,
				requestsDataService.approveWithValidatorsPromise, parameter);
		}

		function denyRequests(replyMessage) {
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Deny, requestsDataService.denyRequestsPromise, replyMessage);
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

		function cancelRequests(replyMessage) {
			vm.ShowCancelAbsenceConfirmationModal = false;
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.Cancel, requestsDataService.cancelRequestsPromise, replyMessage);
		}

		function toggleCancelConfirmationModal() {
			if (canCancelRequests()) {
				vm.ShowCancelAbsenceConfirmationModal = !vm.ShowCancelAbsenceConfirmationModal;
			}
		}

		function toggleProcessWaitlistModal() {
			vm.showProcessWaitlistModal = !vm.showProcessWaitlistModal;
		}

		function displayReplyDialog() {
			getSelectedRequestMessage();
			if (!disableCommands()) {
				vm.showReplyDialog = true;
				vm.showOriginalMessage = getSelectedRequestIds().length == 1 ? true : false;
			}
		}

		function isApproveBasedOnBusinessRulesEnabled() {
			return toggleSvc.Wfm_Requests_Approve_Based_On_Budget_Allotment_39626 && !vm.isShiftTradeViewActive;
		}

		function isRequestsReplyMessageEnabled() {
			return toggleSvc.Wfm_Requests_Reply_Message_39629 && !vm.isShiftTradeViewActive;
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
				onApproveBasedOnBusinessRulesFinished: '&?'
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-commands-pane.tpl.html'
		};
	}
})();