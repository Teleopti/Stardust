(function() {
	'use strict';

	angular
		.module('wfm.requests')
		.controller('requestsCommandsPaneController', requestsCommandsPaneController)
		.directive('requestsCommandsPane', requestsCommandsPaneDirective);

	requestsCommandsPaneController.$inject = [
		'$state',
		'requestsDefinitions',
		'requestsDataService',
		'requestCommandParamsHolder',
		'Toggle',
		'signalRSVC',
		'NoticeService',
		'RequestsCommandsConfigurationsService',
		'requestsPermissions',
		'REQUESTS_TAB_NAMES'
	];

	function requestsCommandsPaneController(
		$state,
		requestsDefinitions,
		requestsDataService,
		requestCommandParamsHolder,
		toggleSvc,
		signalRSVC,
		NoticeService,
		requestsCommandsConfigurationsSvc,
		requestsPermissions,
		REQUESTS_TAB_NAMES
	) {
		var vm = this;
		vm.approveRequests = approveRequests;
		vm.replyRequests = replyRequests;
		vm.displayReplyDialog = displayReplyDialog;
		vm.denyRequests = denyRequests;
		vm.disableCommands = disableCommands;
		vm.canCancelRequests = canCancelRequests;
		vm.toggleCancelConfirmationModal = toggleCancelConfirmationModal;
		vm.toggleProcessWaitlistModal = toggleProcessWaitlistModal;
		vm.processWaitlistRequests = processWaitlistRequests;
		vm.cancelRequests = cancelRequests;
		vm.showApproveBasedOnRulesPanel = false;
		vm.shouldShowResourceCalculations = false;
		vm.lastCaluclated = null;
		vm.toggleApproveBasedOnRulesPanel = toggleApproveBasedOnRulesPanel;
		vm.approveBasedOnBusinessRules = approveBasedOnBusinessRules;
		vm.isApproveBasedOnBusinessRulesEnabled = isApproveBasedOnBusinessRulesEnabled;
		vm.allBusinessRulesForApproving = requestsDataService.getAllBusinessRulesForApproving();
		vm.anyRuleSelected = anyRuleSelected;
		vm.showSiteOpenHour = showSiteOpenHour;
		vm.showResourceCalculations = showResourceCalculations;
		vm.shouldShowSiteOpenHour = false;
		vm.triggerResourceCalculate = triggerResourceCalculate;

		vm.budgetAllowanceDetailIsVisible = false;
		vm.showBudgetAllowanceDetail = showBudgetAllowanceDetail;
		vm.onCloseDialog = onCloseDialog;
		vm.isCommandEnabledWithoutShiftTradeView = isCommandEnabledWithoutShiftTradeView;
		vm.isCommandEnabled = isCommandEnabled;
		vm.showApproveOrDenyRequests = requestsPermissions.all().HasApproveOrDenyPermission;
		vm.showCancelRequests = requestsPermissions.all().HasCancelPermission;
		vm.showReplyRequests = requestsPermissions.all().HasReplyPermission;
		vm.showWaitlist = !toggleSvc.WFM_Request_Remove_Waitlist_Link_78939;

		initWaitlistProcessPeriod();

		subscribeSignalRMessage('IRunRequestWaitlistEventMessage', vm.onProcessWaitlistFinished);
		subscribeSignalRMessage('IApproveRequestsWithValidatorsEventMessage', vm.onApproveBasedOnBusinessRulesFinished);

		function handleErrorMessages(errorMessages) {
			if (vm.onErrorMessages) {
				vm.onErrorMessages({
					errorMessages: errorMessages
				});
			}
		}

		function showSiteOpenHour() {
			vm.shouldShowSiteOpenHour = true;
		}

		function showBudgetAllowanceDetail() {
			vm.budgetAllowanceDetailIsVisible = true;
		}

		function showResourceCalculations() {
			vm.shouldShowResourceCalculations = !vm.shouldShowResourceCalculations;
			requestsDataService.getLastCaluclatedDateTime().success(function(result) {
				result = moment(result).format('MMMM Do YYYY, h:mm:ss a');
				vm.lastCaluclated = result;
			});
		}

		function triggerResourceCalculate() {
			requestsDataService.triggerResourceCalculate().success(function() {
				NoticeService.success('Resource calculation successfully triggered ', 5000, true);
				vm.shouldShowResourceCalculations = false;
			});
		}

		function initWaitlistProcessPeriod() {
			vm.waitlistPeriod = {
				startDate: new Date(),
				endDate: new Date()
			};
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
			if ($state.current.name.indexOf(REQUESTS_TAB_NAMES.overtime) > -1) {
				return requestCommandParamsHolder.getOvertimeSelectedRequestIds();
			}

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
					if (
						requestCommandHandlingResult.Success ||
						(requestCommandHandlingResult.AffectedRequestIds &&
							requestCommandHandlingResult.AffectedRequestIds.length > 0)
					) {
						vm.commandTrackId = requestCommandHandlingResult.CommandTrackId;
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: null,
							waitlistPeriod: waitlistPeriod
						});
					}
					if (
						requestCommandHandlingResult.ErrorMessages &&
						requestCommandHandlingResult.ErrorMessages.length > 0
					) {
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
			var commandInProgress = angular.isUndefined(parameters)
				? dataServicePromise()
				: dataServicePromise(parameters);

			if (vm.afterCommandSuccess) {
				var requestCount = 0;
				if (parameters) {
					if (angular.isArray(parameters)) {
						requestCount = parameters.length;
					} else if (angular.isArray(parameters.SelectedRequestIds)) {
						requestCount = parameters.SelectedRequestIds.length;
					}
				}

				commandInProgress.success(function(requestCommandHandlingResult) {
					if (
						requestCommandHandlingResult.Success ||
						(requestCommandHandlingResult.AffectedRequestIds &&
							requestCommandHandlingResult.AffectedRequestIds.length > 0)
					) {
						vm.commandTrackId = requestCommandHandlingResult.CommandTrackId;
						vm.afterCommandSuccess({
							commandType: requestType,
							changedRequestsCount: requestCommandHandlingResult.AffectedRequestIds.length,
							requestsCount: requestCount
						});
					}
					if (requestCommandHandlingResult.ReplySuccessCount > 0) {
						vm.afterCommandSuccess({
							commandType: requestsDefinitions.REQUEST_COMMANDS.Reply,
							changedRequestsCount: requestCommandHandlingResult.ReplySuccessCount
						});
					}
					if (
						requestCommandHandlingResult.ErrorMessages &&
						requestCommandHandlingResult.ErrorMessages.length > 0
					) {
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
			};
			doStandardCommandHandlingWithParameters(requestType, dataServicePromise, selectedRequestIdsAndMessage);
		}

		function subscribeSignalRMessage(domainType, eventHandler) {
			signalRSVC.subscribe(
				{
					DomainType: domainType
				},
				function(message) {
					if (message.TrackId === vm.commandTrackId) {
						eventHandler({
							message: message
						});
					}
				}
			);
		}

		function replyRequests(message) {
			doStandardCommandHandling(null, requestsDataService.replyRequestsPromise, message);
		}

		function approveRequests(replyMessage) {
			doStandardCommandHandling(
				requestsDefinitions.REQUEST_COMMANDS.Approve,
				requestsDataService.approveRequestsPromise,
				replyMessage
			);
		}

		function processWaitlistRequests() {
			vm.toggleProcessWaitlistModal();
			doProcessWaitlistCommandHandling(vm.waitlistPeriod);
			initWaitlistProcessPeriod();
		}

		function toggleApproveBasedOnRulesPanel() {
			if (!vm.isApproveBasedOnBusinessRulesEnabled() || vm.disableCommands()) return;
			vm.showApproveBasedOnRulesPanel = !vm.showApproveBasedOnRulesPanel;
		}

		function getSelectedRulesFlag() {
			var selectedRulesFlag = requestsDefinitions.REQUEST_VALIDATORS.None;

			angular.forEach(vm.allBusinessRulesForApproving, function(rule) {
				selectedRulesFlag += rule.Checked ? rule.Id : 0;
			});

			return selectedRulesFlag;
		}

		function anyRuleSelected() {
			return getSelectedRulesFlag() > requestsDefinitions.REQUEST_VALIDATORS.None;
		}

		function approveBasedOnBusinessRules() {
			var selectedRulesFlag = getSelectedRulesFlag();
			if (selectedRulesFlag <= 0) return;

			var selectedRequestIds = getSelectedRequestIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;

			var parameter = {
				RequestIds: selectedRequestIds,
				Validators: selectedRulesFlag
			};

			doStandardCommandHandlingWithParameters(
				requestsDefinitions.REQUEST_COMMANDS.ApproveBasedOnBusinessRules,
				requestsDataService.approveWithValidatorsPromise,
				parameter
			);

			vm.showApproveBasedOnRulesPanel = false;
		}

		function denyRequests(replyMessage) {
			doStandardCommandHandling(
				requestsDefinitions.REQUEST_COMMANDS.Deny,
				requestsDataService.denyRequestsPromise,
				replyMessage
			);
		}

		function disableCommands() {
			var selectedRequestIds = getSelectedRequestIds();
			if (vm.commandsDisabled) return true;
			return !selectedRequestIds || selectedRequestIds.length === 0;
		}

		function canCancelRequests() {
			return !disableCommands();
		}

		function cancelRequests(replyMessage) {
			vm.ShowCancelAbsenceConfirmationModal = false;
			doStandardCommandHandling(
				requestsDefinitions.REQUEST_COMMANDS.Cancel,
				requestsDataService.cancelRequestsPromise,
				replyMessage
			);
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
				vm.showOriginalMessage = getSelectedRequestIds().length === 1 ? true : false;
			}
		}

		function isApproveBasedOnBusinessRulesEnabled() {
			return (
				(toggleSvc.Wfm_Requests_Approve_Based_On_Budget_Allotment_39626 ||
					toggleSvc.Wfm_Requests_Approve_Based_On_Intraday_39868 ||
					toggleSvc.Wfm_Requests_Approve_Based_On_Minimum_Approval_Time_40274) &&
				isCommandEnabledWithoutShiftTradeView('approveBasedOnBusinessRules')
			);
		}

		function onCloseDialog() {
			var backdrops = angular.element(document).find('md-backdrop');
			if (backdrops.length) {
				backdrops[0].click();
			}
		}

		function isCommandEnabledWithoutShiftTradeView(commandName) {
			if ($state.current.name.split('.').length == 2) {
				return requestsCommandsConfigurationsSvc.configurations[$state.current.name.split('.')[1]][commandName];
			}

			return !vm.isShiftTradeViewActive;
		}

		function isCommandEnabled(commandName) {
			if ($state.current.name.split('.').length == 2) {
				return requestsCommandsConfigurationsSvc.configurations[$state.current.name.split('.')[1]][commandName];
			}

			return true;
		}
	}

	function requestsCommandsPaneDirective() {
		return {
			controller: 'requestsCommandsPaneController',
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
			templateUrl: 'app/requests/html/requests-commands-pane.tpl.html'
		};
	}
})();
