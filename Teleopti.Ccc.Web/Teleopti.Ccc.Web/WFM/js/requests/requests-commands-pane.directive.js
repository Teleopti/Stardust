(function () {
	'use strict';

	angular.module('wfm.requests')
		.controller('requestsCommandsPaneCtrl', requestsCommandsPaneCtrl)
		.directive('requestsCommandsPane', requestsCommandsPaneDirective)


	requestsCommandsPaneCtrl.$inject = ['requestsDefinitions', 'requestsDataService', 'requestCommandParamsHolder', 'Toggle'];

	function requestsCommandsPaneCtrl(requestsDefinitions, requestsDataService, requestCommandParamsHolder, toggleSvc) {
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

		function doStandardCommandHandling(requestType, dataServicePromise, useStraight, commandId, waitlistPeriod) {

			if (!useStraight) {
				var selectedRequestIds = getSelectedRequestIds();
				if (!selectedRequestIds || selectedRequestIds.length === 0) return;
				if (vm.beforeCommand && !vm.beforeCommand()) return;
				var commandInProgress = dataServicePromise(selectedRequestIds);
			} else {
				var commandInProgress = dataServicePromise(waitlistPeriod, commandId);
			}


			if (vm.afterCommandSuccess) {
				commandInProgress.success(function (requestCommandHandlingResult) {

					if (requestCommandHandlingResult.Success) {
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: useStraight == true ? null : selectedRequestIds.length,
							commandId: commandId,
							waitlistPeriod: waitlistPeriod
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
		function s4() {
			return Math.floor((1 + Math.random()) * 0x10000)
				.toString(16)
				.substring(1);
		}

		var newGuid = function () {
			return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
		};

		var commandId = newGuid();

		function processWaitlistRequests() {
			vm.toggleProcessWaitlistModal();
			doStandardCommandHandling(requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist, requestsDataService.processWaitlistRequestsPromise, true, commandId, vm.waitlistPeriod);
			initWaitlistProcessPeriod();
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

			if(canCancelRequests()) {
				vm.ShowCancelAbsenceConfirmationModal = !vm.ShowCancelAbsenceConfirmationModal;
				}
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


				}) ();