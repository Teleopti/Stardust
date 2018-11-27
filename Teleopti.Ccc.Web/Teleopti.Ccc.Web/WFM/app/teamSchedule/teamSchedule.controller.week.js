(function () {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyController', TeamScheduleWeeklyController);

	TeamScheduleWeeklyController.$inject =
		[
			'$window',
			'$q',
			'$translate',
			'$filter',
			'PersonScheduleWeekViewCreator',
			'UtilityService',
			'weekViewScheduleSvc',
			'TeamSchedule',
			'signalRSVC',
			'$scope',
			'Toggle',
			'bootstrapCommon',
			'groupPageService',
			'serviceDateFormatHelper',
			'ViewStateKeeper',
			'CurrentUserInfo'];

	function TeamScheduleWeeklyController(
		$window,
		$q,
		$translate,
		$filter,
		WeekViewCreator,
		Util,
		weekViewScheduleSvc,
		teamScheduleSvc,
		signalR,
		$scope,
		toggles,
		bootstrapCommon,
		groupPageService,
		serviceDateFormatHelper,
		ViewStateKeeper,
		CurrentUserInfo) {
		var vm = this;
		var stateParams = ViewStateKeeper.get();
		vm.searchOptions = {
			keyword: angular.isDefined(stateParams.keyword) && stateParams.keyword !== '' ? stateParams.keyword : '',
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};

		var mode = {
			BusinessHierarchy: 'BusinessHierarchy',
			GroupPages: 'GroupPages'
		}
		vm.toggles = toggles;
		vm.boostrap = bootstrapCommon.ready();
		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];

		vm.scheduleDate = Util.getFirstDayOfWeek(stateParams.selectedDate || serviceDateFormatHelper.getDateOnly(moment()));
		vm.weekDays = Util.getWeekdays(vm.scheduleDate);

		vm.teamNameMap = stateParams.teamNameMap || {};
		vm.enableClickableCell = true;
		vm.onCellClick = openSelectedAgentDayInNewWindow;
		vm.sortOption = stateParams.selectedSortOption;
		vm.staffingEnabled = stateParams.staffingEnabled;
		vm.timezone = stateParams.timezone;
		initSelectedGroups(mode.BusinessHierarchy, [], '');

		if (angular.isArray(stateParams.selectedTeamIds) && stateParams.selectedTeamIds.length > 0) {
			vm.selectedGroups.groupIds = stateParams.selectedTeamIds.slice(0);
		}
		else if (stateParams.selectedGroupPage && stateParams.selectedGroupPage.groupIds.length > 0) {
			initSelectedGroups(mode.GroupPages, stateParams.selectedGroupPage.groupIds.slice(0), stateParams.selectedGroupPage.pageId);
		}

		vm.selectedFavorite = stateParams.do ? stateParams.selectedFavorite : null;

		vm.onKeyWordInSearchInputChanged = function () {
			vm.selectedFavorite = false;
			vm.resetSchedulePage();
		};

		vm.selectorChanged = function () {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.resetSchedulePage();
			});
		};

		vm.resetSchedulePage = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.scheduleFullyLoaded = false;
			vm.loadSchedules();
			resetFocus();
		};

		vm.onScheduleDateChanged = function () {
			vm.weekDays = Util.getWeekdays(vm.scheduleDate);

			vm.getGroupPagesAsync();
			vm.loadSchedules();
		};

		vm.updateScheduleDate = function (date) {
			return Util.getFirstDayOfWeek(date);
		}

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.isResultTooMany = function () {
			return (toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871 && vm.total > 750)
				|| (!toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871 && vm.total > 500)
		};

		vm.warningMessageForTooManyResuts = function () {
			var toggle = toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871;
			var max = toggle ? 750 : 500;
			return $translate.instant('TooManyResultsForSearchKeywords').replace('{0}', max);
		};

		vm.loadSchedules = function () {
			vm.isLoading = true;
			var inputForm = getParamsForLoadingSchedules();

			weekViewScheduleSvc.getSchedules(inputForm).then(function (data) {
				vm.groupWeeks = WeekViewCreator.Create(data.PersonWeekSchedules);
				vm.paginationOptions.totalPages = vm.paginationOptions.pageSize > 0 ? Math.ceil(data.Total / (vm.paginationOptions.pageSize + 0.01)) : 0;
				vm.isLoading = false;
				vm.scheduleFullyLoaded = true;
				vm.searchOptions.focusingSearch = false;
				vm.total = data.Total;
			}, function () {
				vm.isLoading = false;
				vm.searchOptions.focusingSearch = false;
			});
		};

		vm.onSelectedTeamsChanged = function (teams) {
			stateParams.selectedTeamIds = vm.selectedGroups.groupIds;
			vm.searchOptions.focusingSearch = true;
			vm.selectedFavorite = false;
		};

		var loggedonUsersTeamId = $q.defer();

		vm.onFavoriteSearchInitDefer = $q.defer();

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			initSelectedGroups(mode.BusinessHierarchy, currentFavorite.TeamIds.slice(0), '');
			vm.searchOptions.keyword = currentFavorite.SearchTerm;
			vm.resetSchedulePage();
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedGroups.groupIds) && vm.selectedGroups.groupIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedGroups.mode === 'BusinessHierarchy' ? vm.selectedGroups.groupIds : [],
				SearchTerm: vm.searchOptions.keyword
			};
		};

		vm.paginationOptions.totalPages = 1;

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

		var asyncData = {
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			loggedonUsersTeamId: loggedonUsersTeamId.promise,
			defaultFavoriteSearch: vm.onFavoriteSearchInitDefer.promise
		};

		vm.getGroupPagesAsync = function () {
			var date = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);
			var startOfWeek = Util.getFirstDayOfWeek(vm.scheduleDate);
			var endOfWeek = serviceDateFormatHelper.getDateOnly(moment(startOfWeek).add(6, 'days'));
			groupPageService.fetchAvailableGroupPages(startOfWeek, endOfWeek).then(function (data) {
				vm.availableGroups = data;
				loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
			});
		};

		vm.getSitesAndTeamsAsync = function () {
			return $q(function (resolve, reject) {
				var date = serviceDateFormatHelper.getDateOnly(moment(vm.scheduleDate));
				var startOfWeek = Util.getFirstDayOfWeek(date);
				var endOfWeek = serviceDateFormatHelper.getDateOnly(moment(startOfWeek).add(6, 'days'));

				var promise = toggles.Wfm_HideUnusedTeamsAndSites_42690 ?
					teamScheduleSvc.hierarchyOverPeriod(startOfWeek, endOfWeek)
					: teamScheduleSvc.hierarchy(date);

				promise.then(function (data) {
					resolve(data);
					loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
					vm.sitesAndTeams = data.Children;

					angular.extend(vm.teamNameMap, extractTeamNames(data.Children));
				});
			});
		};

		vm.searchPlaceholder = $translate.instant('Search');

		function getFormatDate(date) {
			return serviceDateFormatHelper.getDateOnly(moment(date));
		}

		function resetFocus() {
			$scope.$broadcast("resetFocus", "organizationPicker");
		};

		function openSelectedAgentDayInNewWindow(personId, scheduleDate) {
			if (!vm.enableClickableCell) return;
			$window.open('#/teams/?personId=' + personId + '&selectedDate=' + serviceDateFormatHelper.getDateOnly(scheduleDate),
				'_blank');
		}

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || serviceDateFormatHelper.getDateOnly(vm.scheduleDate),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber,
				SelectedGroupIds: vm.selectedGroups.groupIds,
				SelectedGroupPageId: vm.selectedGroups.groupPageId
			};
			return params;
		}

		function monitorScheduleChanged() {
			var options = { DomainType: 'IScheduleChangedInDefaultScenario' };
			signalR.subscribeBatchMessage(options, scheduleChangedEventHandler, 300);
		}

		function scheduleChangedEventHandler() {
			$scope.$evalAsync(vm.loadSchedules);
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

		function initSelectedGroups(mode, groupIds, groupPageId) {
			vm.selectedGroups = {
				mode: mode,
				groupIds: groupIds,
				groupPageId: groupPageId
			};
		}

		function init() {
			vm.getGroupPagesAsync();

			$q.all(asyncData).then(function (data) {
				if (data.pageSetting.Agents > 0) {
					vm.paginationOptions.pageSize = data.pageSetting.Agents;
				}

				var defaultFavoriteSearch = data.defaultFavoriteSearch;
				var loggedonUsersTeamId = data.loggedonUsersTeamId;

				if (defaultFavoriteSearch) {
					vm.selectedGroups.groupIds = defaultFavoriteSearch.TeamIds.slice(0);
					vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
					vm.selectedFavorite = defaultFavoriteSearch;
				} else if (loggedonUsersTeamId && vm.selectedGroups.groupIds.length === 0) {
					vm.selectedGroups.groupIds = [loggedonUsersTeamId].slice(0);
				}
				vm.resetSchedulePage();
				monitorScheduleChanged();
			});
		}

		init();
	}
})();