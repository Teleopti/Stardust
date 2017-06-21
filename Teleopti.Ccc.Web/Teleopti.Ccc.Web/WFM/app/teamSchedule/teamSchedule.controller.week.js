(function () {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyController', TeamScheduleWeeklyController);

	TeamScheduleWeeklyController.$inject = ['$window', '$stateParams', '$q', '$translate', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc', 'TeamSchedule', 'signalRSVC', '$scope', 'teamsToggles', 'bootstrapCommon'];

	function TeamScheduleWeeklyController($window, $stateParams, $q, $translate, $filter, WeekViewCreator, Util, weekViewScheduleSvc, teamScheduleSvc, signalR, $scope, teamsToggles, bootstrapCommon) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined($stateParams.keyword) && $stateParams.keyword !== '' ? $stateParams.keyword : '',
			searchKeywordChanged: false,
			focusingSearch: false,
			searchFields: [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};
		vm.boostrap = bootstrapCommon.ready();
		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];
		vm.scheduleDate = $stateParams.selectedDate || new Date();

		Object.defineProperty(this, 'selectedTeamIds', { value: [] });

		if (angular.isArray($stateParams.selectedTeamIds)) {
			replaceArrayValues($stateParams.selectedTeamIds, vm.selectedTeamIds);
		}

		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };

		vm.startOfWeek = moment(vm.scheduleDate).startOf('week').toDate();

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
		};

		vm.onStartOfWeekChanged = function () {
			vm.scheduleDate = new Date(vm.startOfWeek.getTime());
			if (!moment(vm.startOfWeek).startOf('week').isSame(vm.startOfWeek, 'day')) {
				vm.startOfWeek = moment(vm.startOfWeek).startOf('week').toDate();
			}
			vm.weekDays = Util.getWeekdays(vm.startOfWeek);
			vm.loadSchedules();
			if(vm.toggles.Wfm_HideUnusedTeamsAndSites_42690){
				vm.getSitesAndTeamsAsync();
			}
		};

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
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
			}).catch(function () {
				vm.isLoading = false;
				vm.searchOptions.focusingSearch = false;
			});
		};

		vm.onSelectedTeamsChanged = function (teams) {
			$stateParams.selectedTeamIds = vm.selectedTeamIds;
			vm.searchOptions.focusingSearch = true;
			vm.selectedFavorite = false;
		};

		vm.toggles = teamsToggles.all();
		var loggedonUsersTeamId = $q.defer();
		if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled) {
			loggedonUsersTeamId.resolve(null);
		}

		vm.onFavoriteSearchInitDefer = $q.defer();

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedTeamIds);
			vm.searchOptions.keyword = currentFavorite.SearchTerm;
			vm.resetSchedulePage();
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedTeamIds) && vm.selectedTeamIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedTeamIds,
				SearchTerm: vm.searchOptions.keyword
			};
		};
		vm.weekDays = Util.getWeekdays(vm.scheduleDate);
		vm.paginationOptions.totalPages = 1;

		var asyncData = {
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			loggedonUsersTeamId: loggedonUsersTeamId.promise,
			defaultFavoriteSearch: vm.onFavoriteSearchInitDefer.promise
		};

		if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled) {
			loggedonUsersTeamId.resolve(null);
		}

		vm.getSitesAndTeamsAsync = function () {
			return vm._sitesAndTeamsPromise = $q(function (resolve, reject) {
				var date = moment(vm.startOfWeek).format('YYYY-MM-DD');
				teamScheduleSvc.hierarchy(date)
					.then(function (data) {
						resolve(data);
						loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
						vm.sitesAndTeams = data.Children;
					});
			});
		};
		vm.getSitesAndTeamsAsync();
		$q.all(asyncData).then(function (data) {
			if (data.pageSetting.Agents > 0) {
				vm.paginationOptions.pageSize = data.pageSetting.Agents;
			}

			var defaultFavoriteSearch = data.defaultFavoriteSearch;
			var loggedonUsersTeamId = data.loggedonUsersTeamId;

			if (!$stateParams.do) {
				if (defaultFavoriteSearch) {
					replaceArrayValues(defaultFavoriteSearch.TeamIds, vm.selectedTeamIds);
					vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
					vm.selectedFavorite = defaultFavoriteSearch;
				} else if (loggedonUsersTeamId && vm.selectedTeamIds.length === 0) {
					replaceArrayValues([loggedonUsersTeamId], vm.selectedTeamIds);
				}
			}

			vm.resetSchedulePage();
			monitorScheduleChanged();
		});


		if (vm.toggles.WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557) {
			vm.enableClickableCell = true;
			vm.onCellClick = openSelectedAgentDayInNewWindow;
		}

		function openSelectedAgentDayInNewWindow(personId, scheduleDate) {
			if (!vm.enableClickableCell) return;
			$window.open('#/teams/?personId=' + personId + '&selectedDate=' + moment(scheduleDate).format('YYYY-MM-DD'),
				'_blank');
		}

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				SelectedTeamIds: vm.selectedTeamIds ? vm.selectedTeamIds : [],
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || moment(vm.startOfWeek).format('YYYY-MM-DD'),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber
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

		vm.searchPlaceholder = $translate.instant('Search');
	}

	function replaceArrayValues(from, to) {
		to.splice(0);
		from.forEach(function (x) { to.push(x); });
	}
})()