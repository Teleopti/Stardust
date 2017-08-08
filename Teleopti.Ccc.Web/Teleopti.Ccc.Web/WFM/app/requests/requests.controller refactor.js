(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsRefactorCtrl', requestsController);

	requestsController.$inject = ["$scope", "$q", "$translate", "$state", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestsDataService", "requestCommandParamsHolder", "NoticeService", "FavoriteSearchDataService", "CurrentUserInfo"];

	function requestsController($scope, $q, $translate, $state, toggleService, requestsDefinitions, requestsNotificationService, requestsDataService, requestCommandParamsHolder, noticeSvc, FavoriteSearchSvc, CurrentUserInfo) {
		var vm = this;

		vm.searchPlaceholder = $translate.instant('Search');
		vm.translations = {};
		vm.translations.From = $translate.instant('DateFrom');
		vm.translations.To = $translate.instant('DateTo');
		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.sitesAndTeams = [];

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

		vm.teamNameMap = {};

		if (!vm.saveFavoriteSearchesToggleEnabled) {
			vm.onFavoriteSearchInitDefer.resolve();
		}

		Object.defineProperty(this, 'selectedTeamIds', { value: [] });

		var loggedonUsersTeamId = $q.defer();
		if (!toggleService.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309) {
			loggedonUsersTeamId.resolve(null);
		}

		vm.orgPickerSelectedText = function () {
			var text = '';
			switch (vm.selectedTeamIds.length) {
				case 0:
					text = $translate.instant('SelectOrganization');
					break;

				case 1:
					text = vm.teamNameMap[vm.selectedTeamIds[0]];
					break;

				default:
					text = $translate.instant('SeveralTeamsSelected').replace('{0}', vm.selectedTeamIds.length);
					break;
			}
			return text;
		};

		vm.activeAbsenceAndTextTab = function () {
			var params = {
				period: vm.absencePeriod,
				agentSearchTerm: '',
				selectedTeamIds: vm.selectedTeamIds,
				filterEnabled: vm.filterEnabled,
				onInitCallBack: vm.initFooter,
				paging: vm.paging,
				isUsingRequestSubmitterTimeZone: vm.isUsingRequestSubmitterTimeZone
			};

			vm.period = vm.absencePeriod;

			$state.go("requestsRefactor.absenceAndText", params);
		};


		vm.activeShiftTradeTab = function () {
			var params = {
				period: vm.shiftTradePeriod,
				agentSearchTerm: '',
				selectedTeamIds: vm.selectedTeamIds,
				filterEnabled: vm.filterEnabled,
				onInitCallBack: vm.initFooter,
				paging: vm.paging,
				isUsingRequestSubmitterTimeZone: vm.isUsingRequestSubmitterTimeZone
			};

			vm.period = vm.shiftTradePeriod;

			$state.go("requestsRefactor.shiftTrade", params);
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

		$q.all([toggleService.togglesLoaded])
			.then(FavoriteSearchSvc.hasPermission().then(function (response) {
				vm.hasFavoriteSearchPermission = response.data;
			}))
			.then(loggedonUsersTeamId.promise.then(function (defaultTeam) {
				if (angular.isString(defaultTeam) && defaultTeam.length > 0)
					replaceArrayValues([defaultTeam], vm.selectedTeamIds);
				if (vm.businessHierarchyToggleEnabled && (!vm.saveFavoriteSearchesToggleEnabled || !vm.hasFavoriteSearchPermission)) {
					$scope.$broadcast('reload.requests.with.selection', { selectedTeamIds: vm.selectedTeamIds, agentSearchTerm: vm.agentSearchOptions.keyword });
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

		vm.changeSelectedTeams = function () {
			vm.agentSearchOptions.focusingSearch = true;
			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedTeamIds);
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', { selectedTeamIds: currentFavorite.TeamIds, agentSearchTerm: vm.agentSearchOptions.keyword });
			vm.agentSearchOptions.focusingSearch = false;
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedTeamIds,
				SearchTerm: vm.agentSearchOptions.keyword
			};
		};

		vm.onSearchTermChangedCallback = function () {
			vm.agentSearchOptions.focusingSearch = false;

			requestCommandParamsHolder.resetSelectedRequestIds(isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection',
				{
					selectedTeamIds: vm.selectedTeamIds,
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
			if (angular.isArray(vm.selectedTeamIds) && vm.selectedTeamIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		function init() {
			initToggles();
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
			vm.dateRangeTemplateType = 'popup';

			vm.onFavoriteSearchInitDefer.promise.then(function (defaultSearch) {
				if (defaultSearch) {
					replaceArrayValues(defaultSearch.TeamIds, vm.selectedTeamIds);
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
				}
				if (vm.saveFavoriteSearchesToggleEnabled && vm.hasFavoriteSearchPermission) {
					$scope.$broadcast('reload.requests.with.selection', { selectedTeamIds: vm.selectedTeamIds, agentSearchTerm: vm.agentSearchOptions.keyword });
				}

			});

			var defaultDateRange = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			vm.absencePeriod = defaultDateRange;
			vm.shiftTradePeriod = defaultDateRange;
			vm.period = defaultDateRange;

			vm.onBeforeCommand = onBeforeCommand;
			vm.onCommandSuccess = onCommandSuccess;
			vm.onCommandError = onCommandError;
			vm.onErrorMessages = onErrorMessages;
			vm.disableInteraction = false;
			vm.onProcessWaitlistFinished = onProcessWaitlistFinished;
			vm.onApproveBasedOnBusinessRulesFinished = onApproveBasedOnBusinessRulesFinished;
			vm.getSitesAndTeamsAsync();
			setReleaseNotification();
			vm.activeAbsenceAndTextTab();
		}

		function initToggles() {
			vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
			vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
			vm.canApproveOrDenyShiftTradeRequest = toggleService.Wfm_Requests_ApproveDeny_ShiftTrade_38494;
			vm.isShiftTradeViewActive = isShiftTradeViewActive;
			vm.canApproveOrDenyRequest = canApproveOrDenyRequest;
			vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.isShiftTradeViewVisible = toggleService.Wfm_Requests_ShiftTrade_37751;
			vm.filterToggleEnabled = toggleService.Wfm_Requests_Filtering_37748;
			vm.filterEnabled = vm.filterToggleEnabled;
		}

		function setReleaseNotification() {
			if (toggleService.Wfm_Requests_PrepareForRelease_38771) {
				var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
					.replace('{0}', $translate.instant('Requests'))
					.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
					.replace('{2}', '</a>');
				noticeSvc.info(message, null, true);
			}
		}

		function isShiftTradeViewActive() {
			return $state.current.url.indexOf('/requests-refactor/shiftTrade') > -1;
		}

		function canApproveOrDenyRequest() {
			return !isShiftTradeViewActive() || (isShiftTradeViewActive() && vm.canApproveOrDenyShiftTradeRequest);
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

				if (newValue && JSON.stringify(newValue) !== JSON.stringify(oldValue)) {
					if (toggleService.Wfm_HideUnusedTeamsAndSites_42690) {
						vm.getSitesAndTeamsAsync();
					}
				}

				$scope.$broadcast('reload.requests.with.selection',
					{
						period: vm.period
					});
			});
		});

		$scope.$watch(function() {
				return vm.filterEnabled;
		},
		function() {
				$scope.$broadcast('requests.filterEnabled.changed',
					vm.filterEnabled);
		});

		$scope.$watch(function () {
			return vm.isUsingRequestSubmitterTimeZone;
		},
		function () {
			$scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed',
				vm.isUsingRequestSubmitterTimeZone);
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