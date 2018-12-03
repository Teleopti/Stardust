(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsController', RequestsController);

	RequestsController.$inject = [
		'$scope',
		'$state',
		'$q',
		'$translate',
		'$timeout',
		'Toggle',
		'requestsDefinitions',
		'requestsNotificationService',
		'requestsDataService',
		'requestCommandParamsHolder',
		'FavoriteSearchDataService',
		'CurrentUserInfo',
		'groupPageService',
		'requestsPermissions',
		'RequestsFilter'
	];

	function RequestsController(
		$scope,
		$state,
		$q,
		$translate,
		$timeout,
		toggleService,
		requestsDefinitions,
		requestsNotificationService,
		requestsDataService,
		requestCommandParamsHolder,
		FavoriteSearchSvc,
		CurrentUserInfo,
		groupPageService,
		requestsPermissions,
		requestFilterSvc
	) {
		var vm = this;

		vm.searchPlaceholder = $translate.instant('Search');
		vm.translations = {};
		vm.translations.From = $translate.instant('DateFrom');
		vm.translations.To = $translate.instant('DateTo');
		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.sitesAndTeams = [];
		vm.isUsingRequestSubmitterTimeZone = false;
		vm.overtimeRequestsLicenseAvailable = false;
		vm.shiftTradeRequestsLicenseAvailable = false;
		vm.teamNameMap = {};
		vm.showFeedbackLink = toggleService.WFM_Request_Show_Feedback_Link_77733;

		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};

		vm.paging = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 1,
			totalRequestsCount: 0
		};
		vm.agentSearchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: true,
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName',
				'LastName',
				'EmploymentNumber',
				'Organization',
				'Role',
				'Contract',
				'ContractSchedule',
				'ShiftBag',
				'PartTimePercentage',
				'Skill',
				'BudgetGroup',
				'Note'
			]
		};
		vm.onFavoriteSearchInitDefer = $q.defer();

		var loggedonUsersTeamId = $q.defer();
		var shiftTradeMaximumDays = 60;

		vm.orgPickerSelectedText = function () {
			var text = '';
			switch (vm.selectedGroups.groupIds.length) {
				case 0:
					text = $translate.instant('SelectOrganization');
					break;

				case 1:
					text = vm.teamNameMap[vm.selectedGroups.groupIds[0]];
					break;

				default:
					text = $translate.instant('SeveralTeamsSelected').replace('{0}', vm.selectedGroups.groupIds.length);
					break;
			}
			return text;
		};

		var loadRequetsDelay = 300;

		vm.activeAbsenceAndTextTab = function () {
			vm.paging.pageNumber = 1;
			requestCommandParamsHolder.resetAllSelectedRequestsIds();
			vm.period = vm.absenceAndOvertimePeriod;
			vm.selectedTabIndex = 0;
			vm.getTeamOrGroupData();
			$timeout(function () {
				$state.go('requests.absenceAndText', { getParams: getParams });
			}, loadRequetsDelay);
		};

		vm.activeShiftTradeTab = function () {
			vm.paging.pageNumber = 1;
			requestCommandParamsHolder.resetAllSelectedRequestsIds();
			vm.period = vm.shiftTradePeriod;
			vm.selectedTabIndex = 1;
			vm.getTeamOrGroupData();
			$timeout(function () {
				$state.go('requests.shiftTrade', { getParams: getParams });
			}, loadRequetsDelay);
		};

		vm.activeOvertimeTab = function () {
			vm.paging.pageNumber = 1;
			requestCommandParamsHolder.resetAllSelectedRequestsIds();
			vm.period = vm.absenceAndOvertimePeriod;
			vm.selectedTabIndex = 2;
			vm.getTeamOrGroupData();
			$timeout(function () {
				$state.go('requests.overtime', { getParams: getParams });
			}, loadRequetsDelay);
		};

		vm.dateRangeCustomValidators = function () {
			if (
				vm.isShiftTradeViewActive() &&
				moment(vm.period.endDate).diff(moment(vm.period.startDate), 'days') > shiftTradeMaximumDays
			) {
				return $translate.instant('DateRangeIsAMaximumOfSixtyDays');
			}
		};

		vm.changeSelectedTeams = function () {
			vm.agentSearchOptions.focusingSearch = true;
			requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedGroups = { mode: 'BusinessHierarchy', groupIds: [], groupPageId: '' };
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedGroups.groupIds);
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;

			requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', {
				selectedGroupIds: currentFavorite.TeamIds,
				agentSearchTerm: vm.agentSearchOptions.keyword
			});
			vm.agentSearchOptions.focusingSearch = false;
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedGroups.groupIds,
				SearchTerm: vm.agentSearchOptions.keyword
			};
		};

		vm.onSearchTermChangedCallback = function () {
			vm.agentSearchOptions.focusingSearch = false;

			requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', {
				selectedGroupIds: vm.selectedGroups.groupIds,
				selectedGroupPageId: vm.selectedGroups.groupPageId,
				agentSearchTerm: vm.agentSearchOptions.keyword
			});
		};

		vm.initFooter = function (requestsTotalCount) {
			vm.isFooterInited = true;

			var totalPages = Math.ceil(requestsTotalCount / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;

			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = requestsTotalCount;
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedGroups.groupIds) && vm.selectedGroups.groupIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		vm.onBeforeCommand = function () {
			vm.disableInteraction = true;
			return true;
		};

		vm.onCommandSuccess = function (commandType, changedRequestsCount, requestsCount, waitlistPeriod) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
				requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
				requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Cancel) {
				requestsNotificationService.notifyCancelledRequestsSuccess(changedRequestsCount, requestsCount);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist) {
				var period =
					moment(waitlistPeriod.startDate).format('L') + '-' + moment(waitlistPeriod.endDate).format('L');
				requestsNotificationService.notifySubmitProcessWaitlistedRequestsSuccess(period);
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ApproveBasedOnBusinessRules) {
				requestsNotificationService.notifySubmitApproveBasedOnBusinessRulesSuccess();
			} else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Reply) {
				requestsNotificationService.notifyReplySuccess(changedRequestsCount);
			}
		};

		//Todo: submit command failure doesn't give an error info, this parameter will be undefined.
		vm.onCommandError = function (error) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyCommandError(error);
		};

		vm.onProcessWaitlistFinished = function (message) {
			var period = formatDatePeriod(message);
			requestsNotificationService.notifyProcessWaitlistedRequestsFinished(period);
		};

		vm.onApproveBasedOnBusinessRulesFinished = function (message) {
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyApproveBasedOnBusinessRulesFinished();
		};

		vm.onErrorMessages = function (errorMessages) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();

			errorMessages.forEach(function (errorMessage) {
				requestsNotificationService.notifyCommandError(errorMessage);
			});
		};

		vm.isShiftTradeViewActive = function () {
			return $state.current.name.indexOf('requests.shiftTrade') > -1;
		};

		var lastPeriodForFetchingTeamOrGroupData;
		vm.getTeamOrGroupData = function () {
			if (angular.toJson(lastPeriodForFetchingTeamOrGroupData) !== angular.toJson(vm.period)) {
				lastPeriodForFetchingTeamOrGroupData = vm.period;
				getGroupPagesAsync();
			}
		};

		vm.init = function () {
			vm.filterEnabled = true;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
			vm.dateRangeTemplateType = 'popup';
			var defaultDateRange = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			vm.absenceAndOvertimePeriod = defaultDateRange;
			vm.shiftTradePeriod = defaultDateRange;
			vm.period = defaultDateRange;

			vm.getTeamOrGroupData();

			vm.onFavoriteSearchInitDefer.promise.then(function (defaultSearch) {
				if (defaultSearch) {
					replaceArrayValues(defaultSearch.TeamIds, vm.selectedGroups.groupIds);
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
				}
				if (vm.isShiftTradeViewActive()) {
					vm.activeShiftTradeTab();
				} else if ($state.current.name.indexOf('requests.overtime') > -1) {
					vm.activeOvertimeTab();
				} else {
					vm.activeAbsenceAndTextTab();
				}
				setupWatches();
			});

			requestFilterSvc.resetAllFilters();
		};

		$q.all([toggleService.togglesLoaded])
			.then(
			FavoriteSearchSvc.hasPermission().then(function (response) {
				vm.hasFavoriteSearchPermission = response.data;
			})
			)
			.then(
			loggedonUsersTeamId.promise.then(function (defaultTeam) {
				if (angular.isString(defaultTeam) && defaultTeam.length > 0)
					replaceArrayValues([defaultTeam], vm.selectedGroups.groupIds);
			})
			)
			.then(
			requestsDataService.getRequestLicense().then(function (result) {
				vm.overtimeRequestsLicenseAvailable = result.data.IsOvertimeRequestEnabled;
				vm.shiftTradeRequestsLicenseAvailable = result.data.IsShiftTradeRequestEnabled;
			})
			)
			.then(
			requestsDataService.getPermissionsPromise().then(function (result) {
				requestsPermissions.set(result.data);
				vm.permissionInited = true;
			})
			)
			.then(vm.init);

		function forceRequestsReloadWithoutSelection() {
			$scope.$broadcast('reload.requests.without.selection');
		}

		function getSitesAndTeamsAsync() {
			var params = {};
			params.startDate = moment(vm.period.startDate).format('YYYY-MM-DD');
			params.endDate = moment(vm.period.endDate).format('YYYY-MM-DD');

			return (vm._sitesAndTeamsPromise = $q(function (resolve, reject) {
				requestsDataService.hierarchy(params).then(function (data) {
					resolve(data);
					vm.sitesAndTeams = data.Children;
					loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);

					angular.extend(vm.teamNameMap, extractTeamNames(data.Children));
				});
			}));
		}

		function getGroupPagesAsync() {
			var startDateStr = moment(vm.period.startDate)
				.locale('en')
				.format('YYYY-MM-DD');
			var endDateStr = moment(vm.period.endDate)
				.locale('en')
				.format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(startDateStr, endDateStr).then(function (data) {
				vm.availableGroups = data;
				loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
			});
		}

		function formatDatePeriod(message) {
			var userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var startDate = moment(message.StartDate.substring(1, message.StartDate.length))
				.tz(userTimeZone)
				.format('L');
			var endDate = moment(message.EndDate.substring(1, message.EndDate.length))
				.tz(userTimeZone)
				.format('L');
			return startDate + '-' + endDate;
		}

		function getParams() {
			return {
				agentSearchTerm: vm.agentSearchOptions.keyword,
				selectedGroupIds: vm.selectedGroups.groupIds,
				filterEnabled: vm.filterEnabled,
				onInitCallBack: vm.initFooter,
				paging: vm.paging,
				isUsingRequestSubmitterTimeZone: vm.isUsingRequestSubmitterTimeZone,
				getPeriod: function () {
					return vm.period;
				}
			};
		}

		function setupWatches() {
			$scope.$watch(
				function () {
					return vm.period;
				},
				function (newValue, oldValue) {
					$scope.$evalAsync(function () {
						if (vm.isShiftTradeViewActive()) {
							vm.shiftTradePeriod = newValue;
						} else {
							vm.absenceAndOvertimePeriod = newValue;
						}
						requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
						vm.agentSearchOptions.focusingSearch = false;
					});
				}
			);

			$scope.$watch(
				function () {
					return vm.filterEnabled;
				},
				function () {
					$scope.$broadcast('requests.filterEnabled.changed', vm.filterEnabled);
				}
			);

			$scope.$watch(
				function () {
					return vm.isUsingRequestSubmitterTimeZone;
				},
				function (newVal, oldVal) {
					$scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed', newVal);
				}
			);
		}

		function replaceArrayValues(from, to) {
			to.splice(0);
			from.forEach(function (x) {
				to.push(x);
			});
		}

		function extractTeamNames(sites) {
			var teamNameMap = {};
			sites.forEach(function (site) {
				site.Children.forEach(function (team) {
					teamNameMap[team.Id] = team.Name;
				});
			});
			return teamNameMap;
		}
	}
})();
