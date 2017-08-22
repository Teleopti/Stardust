(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsRefactorCtrl', requestsController);

	requestsController.$inject = ["$scope", "$state", "$q", "$translate", "Toggle", "requestsDefinitions", "requestsNotificationService", "requestsDataService", "requestCommandParamsHolder", "NoticeService", "FavoriteSearchDataService", "CurrentUserInfo", "groupPageService"];

	function requestsController($scope, $state, $q, $translate, toggleService, requestsDefinitions, requestsNotificationService, requestsDataService, requestCommandParamsHolder, noticeSvc, FavoriteSearchSvc, CurrentUserInfo, groupPageService) {
		var vm = this;

		vm.searchPlaceholder = $translate.instant('Search');
		vm.translations = {};
		vm.translations.From = $translate.instant('DateFrom');
		vm.translations.To = $translate.instant('DateTo');
		vm.pageSizeOptions = [20, 50, 100, 200];
		vm.sitesAndTeams = [];
		vm.Wfm_GroupPages_45057 = toggleService.Wfm_GroupPages_45057;

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
		vm.onFavoriteSearchInitDefer = $q.defer();

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

		vm.activeAbsenceAndTextTab = function () {
			var params = {
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

			vm.selectedTabIndex = 0;

			vm.period = vm.absencePeriod;
			$state.go("requestsRefactor-absenceAndText", params);
		};

		vm.activeShiftTradeTab = function () {
			var params = {
				agentSearchTerm: '',
				selectedGroupIds: vm.selectedGroups.groupIds,
				filterEnabled: vm.filterEnabled,
				onInitCallBack: vm.initFooter,
				paging: vm.paging,
				isUsingRequestSubmitterTimeZone: vm.isUsingRequestSubmitterTimeZone,
				getPeriod: function () {
					return vm.period;
				}
			};

			vm.selectedTabIndex = 1;
			vm.period = vm.shiftTradePeriod;
			$state.go("requestsRefactor-shiftTrade", params);
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

		vm.dateRangeCustomValidators = [{
			key: 'max60Days',
			message: 'DateRangeIsAMaximumOfSixtyDays',
			validate: function (start, end) {
				return !vm.isShiftTradeViewActive() || moment(end).diff(moment(start), 'days') <= 60;
			}
		}];

		vm.changeSelectedTeams = function () {
			vm.agentSearchOptions.focusingSearch = true;
			requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedGroups = { mode: 'BusinessHierarchy', groupIds: [], groupPageId: '' };
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedGroups.groupIds);
			vm.agentSearchOptions.keyword = currentFavorite.SearchTerm;

			requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
			$scope.$broadcast('reload.requests.with.selection', { selectedGroupIds: currentFavorite.TeamIds, agentSearchTerm: vm.agentSearchOptions.keyword });
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
			$scope.$broadcast('reload.requests.with.selection',
				{
					selectedGroupIds: vm.selectedGroups.groupIds,
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

		vm.onBeforeCommand = function() {
			vm.disableInteraction = true;
			return true;
		};

		vm.onCommandSuccess = function(commandType, changedRequestsCount, requestsCount, waitlistPeriod) {
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
		};

		//Todo: submit command failure doesn't give an error info, this parameter will be undefined.
		vm.onCommandError = function(error) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyCommandError(error);
		};

		vm.onProcessWaitlistFinished = function(message) {
			var period = formatDatePeriod(message);
			requestsNotificationService.notifyProcessWaitlistedRequestsFinished(period);
		};

		vm.onApproveBasedOnBusinessRulesFinished = function(message) {
			forceRequestsReloadWithoutSelection();
			requestsNotificationService.notifyApproveBasedOnBusinessRulesFinished();
		};

		vm.onErrorMessages = function(errorMessages) {
			vm.disableInteraction = false;
			forceRequestsReloadWithoutSelection();

			errorMessages.forEach(function (errorMessage) {
				requestsNotificationService.notifyCommandError(errorMessage);
			});
		};

		vm.isShiftTradeViewActive = function() {
			return $state.current.name.indexOf('requestsRefactor-shiftTrade') > -1;
		};

		vm.init = function() {
			vm.filterEnabled = true;
			vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
			vm.dateRangeTemplateType = 'popup';
			var defaultDateRange = {
				startDate: moment().startOf('week')._d,
				endDate: moment().endOf('week')._d
			};
			vm.absencePeriod = defaultDateRange;
			vm.shiftTradePeriod = defaultDateRange;
			vm.period = defaultDateRange;

			if (vm.Wfm_GroupPages_45057)
				vm.getGroupPagesAsync();
			else 
				vm.getSitesAndTeamsAsync();

			setReleaseNotification();
			vm.onFavoriteSearchInitDefer.promise.then(function(defaultSearch) {
				if (defaultSearch) {
					replaceArrayValues(defaultSearch.TeamIds, vm.selectedGroups.groupIds);
					vm.agentSearchOptions.keyword = defaultSearch.SearchTerm;
				}
				if (vm.isShiftTradeViewActive()) {
					vm.activeShiftTradeTab();
				} else {
					vm.activeAbsenceAndTextTab();
				}
				setupWatches();
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
			.then(vm.init);

		function setReleaseNotification() {
			var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
				.replace('{0}', $translate.instant('Requests'))
				.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
				.replace('{2}', '</a>');
			noticeSvc.info(message, null, true);
		}

		function forceRequestsReloadWithoutSelection() {
			$scope.$broadcast('reload.requests.without.selection');
		}

		function formatDatePeriod(message) {
			var userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var startDate = moment(message.StartDate.substring(1, message.StartDate.length)).tz(userTimeZone).format("L");
			var endDate = moment(message.EndDate.substring(1, message.EndDate.length)).tz(userTimeZone).format("L");
			return startDate + "-" + endDate;
		}

		function setupWatches() {
			$scope.$watch(function() {
				return vm.period;
			}, function(newValue, oldValue) {
				$scope.$evalAsync(function() {
					if (vm.isShiftTradeViewActive()) {
						vm.shiftTradePeriod = newValue;
					} else {
						vm.absencePeriod = newValue;
					}
					requestCommandParamsHolder.resetSelectedRequestIds(vm.isShiftTradeViewActive());
					vm.agentSearchOptions.focusingSearch = false;

					if (newValue && JSON.stringify(newValue) !== JSON.stringify(oldValue)) {
						if (toggleService.Wfm_HideUnusedTeamsAndSites_42690) {
							vm.getSitesAndTeamsAsync();
						}
					}
				});
			});

			$scope.$watch(function() {
				return vm.filterEnabled;
			}, function() {
				$scope.$broadcast('requests.filterEnabled.changed',
					vm.filterEnabled);
			});

			$scope.$watch(function() {
				return vm.isUsingRequestSubmitterTimeZone;
			}, function() {
				$scope.$broadcast('requests.isUsingRequestSubmitterTimeZone.changed',
					vm.isUsingRequestSubmitterTimeZone);
			});
		}

		function replaceArrayValues(from, to) {
			to.splice(0);
			from.forEach(function (x) { to.push(x); });
		}
	}
})();