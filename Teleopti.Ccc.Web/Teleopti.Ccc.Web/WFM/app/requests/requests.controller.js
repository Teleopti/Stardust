(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "$q", "$translate", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestsDataService", "requestCommandParamsHolder", "NoticeService", "CurrentUserInfo"];

	function requestsController($scope, $q, $translate, toggleService, requestsDefinitions, requestsNotificationService, requestsDataService, requestCommandParamsHolder, noticeSvc, CurrentUserInfo) {
		var vm = this;
		vm.permissionsReady = false;
		vm.toggleSearchFocus = false;

		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.paging = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 1,
			totalRequestsCount: 0
		};
		vm.initFooter = initFooter;
		function initFooter(count) {
			vm.isFooterInited = true;
			onTotalRequestsCountChanges(count);
		}
		function onTotalRequestsCountChanges(totalRequestsCount) {
			var totalPages = Math.ceil(totalRequestsCount / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;

			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = totalRequestsCount;
		}

		var periodForAbsenceRequest, periodForShiftTradeRequest;
		var absenceRequestTabIndex = 0;
		var shiftTradeRequestTabIndex = 1;
		vm.selectedTeamIds = [];
		var internalSelectedTeamIds = [];

		vm.defaultTeamLoadedDefer = $q.defer();
		if (!toggleService.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309) {
			vm.defaultTeamLoadedDefer.resolve();
		}

		$q.all([toggleService.togglesLoaded])
			.then(vm.defaultTeamLoadedDefer.promise.then(function (defaultTeams) {
				internalSelectedTeamIds = defaultTeams ? defaultTeams : [];
				vm.selectedTeamIds = internalSelectedTeamIds;
				$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds: internalSelectedTeamIds, agentSearchTerm: vm.agentSearchTerm});
			}))
			.then(init);

		vm.dateRangeCustomValidators = [{
			key: 'max60Days',
			message: 'DateRangeIsAMaximumOfSixtyDays',
			validate: function (start, end) {
				return !vm.isShiftTradeViewActive() || moment(end).diff(moment(start), 'days') <= 60;
			}
		}];

		vm.selectedFavorite = null;

		vm.changeSelectedTeams = function (teams) {
			internalSelectedTeamIds = teams;
			vm.focusSearch();
			vm.activeSearchIcon();
			vm.selectedFavorite = false;
			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			internalSelectedTeamIds = currentFavorite.TeamIds;
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;
			setSearchFilter();

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());

			$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds:currentFavorite.TeamIds,agentSearchTerm:currentFavorite.SearchTerm});
			vm.resetSearchStatus();
		};

		vm.resetFocusSearch = function(){
			vm.toggleSearchFocus = false;
		};

		vm.resetSearchStatus = function(){
			vm.resetFocusSearch();
			vm.deactiveSearchIcon();
		};

		vm.focusSearch = function(){
			vm.toggleSearchFocus = true;
		};

		vm.activeSearchIcon = function($event){
			vm.activeSearchIconColor = true;
			if($event && $event.which == 13)
				vm.deactiveSearchIcon();
			setSearchFilter();
		};

		vm.deactiveSearchIcon = function(){
			vm.activeSearchIconColor = false;
		};

		vm.getSearch = function () {
			return {
				TeamIds: internalSelectedTeamIds,
				SearchTerm: vm.agentSearchOptions.keyword
			};
		};

		vm.keyDownOnSearchTermChanged = function() {
			setSearchFilter();
			vm.selectedFavorite = false;
			vm.resetSearchStatus();

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());

			$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds:vm.selectedTeamIds,agentSearchTerm:vm.agentSearchTerm});
		};

		vm.onFavoriteSearchInitDefer = $q.defer();

		function setSearchFilter() {
			vm.selectedTeamIds = internalSelectedTeamIds;
			vm.agentSearchTerm = vm.agentSearchOptions && vm.agentSearchOptions.keyword;
		}

		function init() {
			vm.permissionsReady = true;
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
			vm.businessHierarchyToggleEnabled = toggleService.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309;
			vm.saveFavoriteSearchesEnabled = toggleService.Wfm_Requests_SaveFavoriteSearches_42578;

			if (!vm.saveFavoriteSearchesEnabled) {
				vm.onFavoriteSearchInitDefer.resolve();
			}

			vm.onFavoriteSearchInitDefer.promise.then(function(defaultSearch) {
				if (defaultSearch) {
					vm.selectedTeamIds = defaultSearch.TeamIds;
					internalSelectedTeamIds = defaultSearch.TeamIds;
					
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
					vm.agentSearchTerm = vm.agentSearchOptions.keyword;
					$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds:internalSelectedTeamIds,agentSearchTerm:vm.agentSearchTerm});
				}
			});

			var defaultDateRange = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			vm.absencePeriod = defaultDateRange;
			vm.shiftTradePeriod = defaultDateRange;
			periodForAbsenceRequest = defaultDateRange;
			periodForShiftTradeRequest = defaultDateRange;

			vm.agentSearchOptions = {
				keyword: "",
				isAdvancedSearchEnabled: true,
				searchKeywordChanged: false,
				searchFields: [
					'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
					'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
				]
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
					periodForAbsenceRequest = vm.absencePeriod;
				} else if (previousTab === shiftTradeRequestTabIndex) {
					periodForShiftTradeRequest = vm.shiftTradePeriod;
				}
			}

			if (vm.selectedTabIndex === absenceRequestTabIndex) {
				vm.absencePeriod = periodForAbsenceRequest;
			} else if (vm.selectedTabIndex === shiftTradeRequestTabIndex) {
				vm.shiftTradePeriod = periodForShiftTradeRequest;
			}

			vm.period = isShiftTradeViewActive() ? vm.shiftTradePeriod : vm.absencePeriod;
		});

		$scope.$watch(function() {
			return vm.period;
		}, function (newValue) {
			$scope.$evalAsync(function () {
				if (isShiftTradeViewActive()) {
					vm.shiftTradePeriod = newValue;
				} else {
					vm.absencePeriod = newValue;
				}

				requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
				vm.resetSearchStatus();
			});
		});
	}
})();