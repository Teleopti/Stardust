(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = ["$scope", "$q", "$translate", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestsDataService", "requestCommandParamsHolder", "NoticeService", "FavoriteSearchDataService", "CurrentUserInfo"];

	function requestsController($scope, $q, $translate, toggleService, requestsDefinitions, requestsNotificationService, requestsDataService, requestCommandParamsHolder, noticeSvc, FavoriteSearchSvc, CurrentUserInfo) {
		var vm = this;

		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.paging = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 1,
			totalRequestsCount: 0
		};
		vm.agentSearchOptions = {
			keyword: "",
			isAdvancedSearchEnabled: true,
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};
		vm.businessHierarchyToggleEnabled = toggleService.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309;
		vm.saveFavoriteSearchesToggleEnabled = toggleService.Wfm_Requests_SaveFavoriteSearches_42578;
		vm.hasFavoriteSearchPermission = false;
		vm.onFavoriteSearchInitDefer = $q.defer();

		if (!vm.saveFavoriteSearchesToggleEnabled) {
			vm.onFavoriteSearchInitDefer.resolve();
		}

		var periodForAbsenceRequest, periodForShiftTradeRequest;
		var absenceRequestTabIndex = 0;
		var shiftTradeRequestTabIndex = 1;
		vm.selectedTeamIds = [];
		var loggedonUsersTeamId = $q.defer();
		if (!toggleService.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309) {
			loggedonUsersTeamId.resolve(null);
		}

		vm.sitesAndTeamsAsync = function () {
			vm._sitesAndTeamsPromise = vm._sitesAndTeamsPromise || $q(function (resolve, reject) {
				var date = moment().format('YYYY-MM-DD');
				requestsDataService.hierarchy(date).then(function (data) {
					resolve(data);
					loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
				});
			});
			return vm._sitesAndTeamsPromise;
		};

		$q.all([toggleService.togglesLoaded])
			.then(FavoriteSearchSvc.hasPermission().then(function(response){
				vm.hasFavoriteSearchPermission = response.data;
			}))
			.then(loggedonUsersTeamId.promise.then(function (defaultTeam) {
				vm.selectedTeamIds = (angular.isString(defaultTeam) && defaultTeam.length > 0) ? [defaultTeam] : [];
				if(vm.businessHierarchyToggleEnabled && (!vm.saveFavoriteSearchesToggleEnabled || !vm.hasFavoriteSearchPermission)){
					$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds: vm.selectedTeamIds, agentSearchTerm: vm.agentSearchOptions.keyword});
				}
			}))
			.then(init);

		vm.dateRangeCustomValidators = [{
			key: 'max60Days',
			message: 'DateRangeIsAMaximumOfSixtyDays',
			validate: function (start, end) {
				return !vm.isShiftTradeViewActive() || moment(end).diff(moment(start), 'days') <= 60;
			}
		}];

		vm.changeSelectedTeams = function (teams) {
			vm.selectedTeamIds = teams;
			vm.agentSearchOptions.focusingSearch = true;
			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedTeamIds = currentFavorite.TeamIds;
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds:currentFavorite.TeamIds,agentSearchTerm: vm.agentSearchOptions.keyword});
			vm.agentSearchOptions.focusingSearch = false;
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedTeamIds,
				SearchTerm: vm.agentSearchOptions.keyword
			};
		};

		vm.onSearchTermChangedCallback = function() {
			vm.agentSearchOptions.focusingSearch = false;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds: vm.selectedTeamIds, agentSearchTerm: vm.agentSearchOptions.keyword});
		};

		vm.initFooter = function (count) {
			vm.isFooterInited = true;

			var totalPages = Math.ceil(count / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;

			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = count;
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedTeamIds) && vm.selectedTeamIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

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

			vm.onFavoriteSearchInitDefer.promise.then(function(defaultSearch) {
				if (defaultSearch) {
					vm.selectedTeamIds = defaultSearch.TeamIds;
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
				}
				if(vm.saveFavoriteSearchesToggleEnabled && vm.hasFavoriteSearchPermission){
					$scope.$broadcast('reload.requests.with.selection',{selectedTeamIds: vm.selectedTeamIds, agentSearchTerm: vm.agentSearchOptions.keyword});
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

			vm.onBeforeCommand = onBeforeCommand;
			vm.onCommandSuccess = onCommandSuccess;
			vm.onCommandError = onCommandError;
			vm.onErrorMessages = onErrorMessages;
			vm.disableInteraction = false;
			vm.onProcessWaitlistFinished = onProcessWaitlistFinished;
			vm.onApproveBasedOnBusinessRulesFinished = onApproveBasedOnBusinessRulesFinished;

			setReleaseNotification();
		}

		function setReleaseNotification(){
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
				vm.agentSearchOptions.focusingSearch = false;
			});
		});
	}
})();