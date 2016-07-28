(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "$translate", "Toggle", "requestsDefinitions", "requestsNotificationService", "NoticeService", "CurrentUserInfo"];

	function requestsController($scope, $translate, toggleService, requestsDefinitions, requestsNotificationService, noticeSvc, CurrentUserInfo) {
		var vm = this;
		vm.onAgentSearchTermChanged = onAgentSearchTermChanged;

		var periodForAbsenceRequest, periodForShiftTradeRequest;
		var absenceRequestTabIndex = 0;
		var shiftTradeRequestTabIndex = 1;

		toggleService.togglesLoaded.then(init);

		vm.dateRangeCustomValidators = [{
			key: 'max60Days',
			message: 'DateRangeIsAMaximumOfSixtyDays',
			validate: function (start, end) {
				return !vm.isShiftTradeViewActive() || moment(end).diff(moment(start), 'days') <= 60;
			}
		}];
		
		function init() {
			vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
			vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
			vm.canApproveOrDenyShiftTradeRequest = toggleService.Wfm_Requests_ApproveDeny_ShiftTrade_38494;
			vm.isShiftTradeViewActive = isShiftTradeViewActive;
			vm.canApproveOrDenyRequest = canApproveOrDenyRequest;
			vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.isShiftTradeViewVisible = toggleService.Wfm_Requests_ShiftTrade_37751;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
		
			vm.dateRangeTemplateType = 'popup';
			
			vm.filterToggleEnabled = toggleService.Wfm_Requests_Filtering_37748;
			vm.filterEnabled = vm.filterToggleEnabled;

			var defaultDateRange = {
				startDate: moment().add(-3, 'day').toDate(),
				endDate: moment().add(+3, 'day').toDate()
			};
			vm.period = defaultDateRange;
			periodForAbsenceRequest = defaultDateRange;
			periodForShiftTradeRequest = defaultDateRange;

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
			vm.onProcessWaitlistFinished = onProcessWaitlistFinished;
			vm.onApproveBasedOnBusinessRulesFinished = onApproveBasedOnBusinessRulesFinished;

			if (toggleService.Wfm_Requests_PrepareForRelease_38771) {
				var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
					.replace('{0}', $translate.instant('Requests'))
					.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
					.replace('{2}', '</a>');
				noticeSvc.info(message, null, true);
			}
		}

		function isShiftTradeViewActive() {
			return vm.selectedTabIndex === shiftTradeRequestTabIndex;
		}

		function canApproveOrDenyRequest() {
			return (vm.selectedTabIndex === absenceRequestTabIndex) ||
				(vm.selectedTabIndex === shiftTradeRequestTabIndex && vm.canApproveOrDenyShiftTradeRequest);
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

		function onCommandSuccess(commandType, changedRequestsCount, requestsCount, waitlistPeriod) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
				requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
				requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Cancel) {
				requestsNotificationService.notifyCancelledRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist) {
				var period = moment(waitlistPeriod.startDate).format("L") + "-" + moment(waitlistPeriod.endDate).format("L");
				requestsNotificationService.notifySubmitProcessWaitlistedRequestsSuccess(period);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ApproveBasedOnBusinessRules) {
				requestsNotificationService.notifySubmitApproveBasedOnBusinessRulesSuccess();
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Reply) {
				requestsNotificationService.notifyReplySuccess(changedRequestsCount);
			}
		}

		function onProcessWaitlistFinished(message) {
			var period = formatDatePeriod(message);
			requestsNotificationService.notifyProcessWaitlistedRequestsFinished(period);
		}

		function onApproveBasedOnBusinessRulesFinished(message) {
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyApproveBasedOnBusinessRulesFinished();
		}

		function onErrorMessages(errorMessages) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();

			errorMessages.forEach(function (errorMessage) {
				requestsNotificationService.notifyCommandError(errorMessage);
			});
		}

		function formatDatePeriod(message) {
			var userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var startDate = moment(message.StartDate.substring(1, message.StartDate.length)).tz(userTimeZone).format("L");
			var endDate = moment(message.EndDate.substring(1, message.EndDate.length)).tz(userTimeZone).format("L");
			return startDate + "-" + endDate;
		}

		//Todo: submit command failure doesn't give an error info, this parameter will be undefined.
		function onCommandError(error) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyCommandError(error);
		}

		$scope.$watch(function () {
			return vm.selectedTabIndex;
		}, function (currentTab, previousTab) {
			if (vm.period != undefined) {
				if (previousTab === absenceRequestTabIndex) {
					periodForAbsenceRequest = vm.period;
				} else if (previousTab === shiftTradeRequestTabIndex) {
					periodForShiftTradeRequest = vm.period;
				}
			}

			if (currentTab === absenceRequestTabIndex) {
				vm.period = periodForAbsenceRequest;
			} else if (currentTab === shiftTradeRequestTabIndex) {
				vm.period = periodForShiftTradeRequest;
			}
		});
	}
})();