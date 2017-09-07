(function () {
	'use strict';

	angular.module('wfm.requests').controller('requestsOriginController', requestsOriginController);

	requestsOriginController.$inject = ["$scope", "$q", "$translate", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestsDataService", "requestCommandParamsHolder", "NoticeService", "FavoriteSearchDataService", "CurrentUserInfo", "groupPageService"];

	function requestsOriginController($scope, $q, $translate, toggleService, requestsDefinitions, requestsNotificationService, requestsDataService, requestCommandParamsHolder, noticeSvc, FavoriteSearchSvc, CurrentUserInfo, groupPageService) {
		var vm = this;

		vm.searchPlaceholder = $translate.instant('Search');
		vm.translations = {};
		vm.translations.From = $translate.instant('DateFrom');
		vm.translations.To = $translate.instant('DateTo');
		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.sitesAndTeams = [];

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
			keyword: "",
			isAdvancedSearchEnabled: true,
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};
		vm.Wfm_GroupPages_45057 = toggleService.Wfm_GroupPages_45057;
		vm.hasFavoriteSearchPermission = false;
		vm.onFavoriteSearchInitDefer = $q.defer();

		vm.teamNameMap = {};

		var periodForAbsenceRequest, periodForShiftTradeRequest;
		var absenceRequestTabIndex = 0;
		var shiftTradeRequestTabIndex = 1;

		var loggedonUsersTeamId = $q.defer();

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

		vm.getSitesAndTeamsAsync = function () {
			var params = {};
			if (toggleService.Wfm_HideUnusedTeamsAndSites_42690) {
				params.startDate = moment(vm.period.startDate).format('YYYY-MM-DD');
				params.endDate = moment(vm.period.endDate).format('YYYY-MM-DD');
			} else {
				params.date = moment().format('YYYY-MM-DD');
			}
			return vm._sitesAndTeamsPromise = $q(function (resolve, reject) {
				requestsDataService.hierarchy(params).then(function (data) {
					resolve(data);
					vm.sitesAndTeams = data.Children;
					loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);

					angular.extend(vm.teamNameMap, extractTeamNames(data.Children));
				});
			});
		};

		vm.getGroupPagesAsync = function () {
			var startDateStr = moment(vm.period.startDate).format('YYYY-MM-DD');
			var endDateStr = moment(vm.period.endDate).format('YYYY-MM-DD');

			groupPageService.fetchAvailableGroupPages(startDateStr, endDateStr).then(function (data) {
				vm.availableGroups = data;
				loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
			});
		};

		$q.all([toggleService.togglesLoaded])
			.then(FavoriteSearchSvc.hasPermission().then(function (response) {
				vm.hasFavoriteSearchPermission = response.data;
			}))
			.then(loggedonUsersTeamId.promise.then(function (defaultTeam) {
				if (angular.isString(defaultTeam) && defaultTeam.length > 0)
					replaceArrayValues([defaultTeam], vm.selectedGroups.groupIds);
			}))
			.then(init);

		vm.dateRangeCustomValidators = [{
			key: 'max60Days',
			message: 'DateRangeIsAMaximumOfSixtyDays',
			validate: function (start, end) {
				return !vm.isShiftTradeViewActive() || moment(end).diff(moment(start), 'days') <= 60;
			}
		}];

		vm.changeSelectedTeams = function () {
			vm.agentSearchOptions.focusingSearch = true;
			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedGroups = { mode: 'BusinessHierarchy', groupIds: [], groupPageId: '' };
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedGroups.groupIds);
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', { selectedTeamIds: currentFavorite.TeamIds, agentSearchTerm: vm.agentSearchOptions.keyword });
			vm.agentSearchOptions.focusingSearch = false;
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedGroups.mode === 'BusinessHierarchy' ? vm.selectedGroups.groupIds : [],
				SearchTerm: vm.agentSearchOptions.keyword
			};
		};

		vm.onSearchTermChangedCallback = function () {
			vm.agentSearchOptions.focusingSearch = false;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', { selectedTeamIds: vm.selectedGroups.groupIds, agentSearchTerm: vm.agentSearchOptions.keyword });
		};

		vm.initFooter = function (count) {
			vm.isFooterInited = true;

			var totalPages = Math.ceil(count / vm.paging.pageSize);
			if (totalPages !== vm.paging.totalPages) vm.paging.pageNumber = 1;

			vm.paging.totalPages = totalPages;
			vm.paging.totalRequestsCount = count;
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedGroups.groupIds) && vm.selectedGroups.groupIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		function init() {
			vm.isShiftTradeViewActive = isShiftTradeViewActive;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;

			vm.dateRangeTemplateType = 'popup';
			vm.filterEnabled = true;

			vm.onFavoriteSearchInitDefer.promise.then(function (defaultSearch) {
				if (defaultSearch) {
					replaceArrayValues(defaultSearch.TeamIds, vm.selectedGroups.groupIds);
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
				}
				$scope.$broadcast('reload.requests.with.selection', { selectedGroupIds: vm.selectedGroups.groupIds, agentSearchTerm: vm.agentSearchOptions.keyword });
			});

			var defaultDateRange = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			vm.absencePeriod = defaultDateRange;
			vm.shiftTradePeriod = defaultDateRange;
			vm.period = defaultDateRange;
			periodForAbsenceRequest = defaultDateRange;
			periodForShiftTradeRequest = defaultDateRange;

			vm.onBeforeCommand = onBeforeCommand;
			vm.onCommandSuccess = onCommandSuccess;
			vm.onCommandError = onCommandError;
			vm.onErrorMessages = onErrorMessages;
			vm.disableInteraction = false;
			vm.onProcessWaitlistFinished = onProcessWaitlistFinished;
			vm.onApproveBasedOnBusinessRulesFinished = onApproveBasedOnBusinessRulesFinished;

			if (vm.Wfm_GroupPages_45057)
				vm.getGroupPagesAsync();
			else
				vm.getSitesAndTeamsAsync();

			setReleaseNotification();
		}

		function setReleaseNotification() {
			var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
				.replace('{0}', $translate.instant('Requests'))
				.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
				.replace('{2}', '</a>');
			noticeSvc.info(message, null, true);
		}

		function isShiftTradeViewActive() {
			return vm.selectedTabIndex === shiftTradeRequestTabIndex;
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
			if (vm.period) {
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

		$scope.$watch(function () {
			return vm.period;
		}, function (newValue, oldValue) {
			$scope.$evalAsync(function () {
				if (isShiftTradeViewActive()) {
					vm.shiftTradePeriod = newValue;
				} else {
					vm.absencePeriod = newValue;
				}
				requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
				vm.agentSearchOptions.focusingSearch = false;

				if (newValue && angular.toJson(newValue) !== angular.toJson(oldValue)) {
					if (toggleService.Wfm_HideUnusedTeamsAndSites_42690 && !toggleService.Wfm_GroupPages_45057) {
						vm.getSitesAndTeamsAsync();
					} else {
						vm.getGroupPagesAsync();
					}
				}
			});
		});
	}

	function replaceArrayValues(from, to) {
		to.splice(0);
		from.forEach(function (x) { to.push(x); });
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
})();