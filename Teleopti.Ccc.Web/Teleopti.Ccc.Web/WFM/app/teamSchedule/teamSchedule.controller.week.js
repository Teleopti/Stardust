﻿(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyController', TeamScheduleWeeklyController);

	TeamScheduleWeeklyController.$inject = ['$window', '$stateParams', '$q', '$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc', 'TeamSchedule', 'signalRSVC', '$scope', 'teamsToggles', 'bootstrapCommon'];

	function TeamScheduleWeeklyController($window, $stateParams, $q, $locale, $filter, WeekViewCreator, Util, weekViewScheduleSvc, teamScheduleSvc, signalR, $scope, teamsToggles, bootstrapCommon) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined($stateParams.keyword) && $stateParams.keyword !== '' ? $stateParams.keyword : '',
			searchKeywordChanged: false,
			searchFields : [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};
		vm.boostrap = bootstrapCommon.ready();
		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];
		vm.scheduleDate = $stateParams.selectedDate || new Date();
		vm.selectedTeamIds = $stateParams.selectedTeamIds || [];
		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };

		vm.startOfWeek = moment(vm.scheduleDate).startOf('week').toDate();
	
		vm.onKeyWordInSearchInputChanged = function() {
			vm.selectedFavorite = false;
			vm.resetSchedulePage();
		};

		vm.selectorChanged = function () {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.resetSchedulePage();
			});
		};

		vm.resetFocusSearch = function(){
			vm.focusToSearch = false;
		};

		vm.resetSearchStatus = function(){
			vm.resetFocusSearch();
			vm.deactiveSearchIcon();
		};

		vm.focusSearch = function(){
			vm.focusToSearch = true;
		};

		vm.activeSearchIcon = function($event){
			vm.activeSearchIconColor = true;
			if($event && $event.which == 13)
				vm.deactiveSearchIcon();
		};

		vm.deactiveSearchIcon = function(){
			vm.activeSearchIconColor = false;
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
				vm.paginationOptions.totalPages = vm.paginationOptions.pageSize > 0? Math.ceil(data.Total / (vm.paginationOptions.pageSize + 0.01) ) : 0;
				vm.isLoading = false;
				vm.scheduleFullyLoaded = true;
			}).catch(function() {
				vm.isLoading = false;
			});
			vm.resetSearchStatus();
		};

		vm.onSelectedTeamsChanged = function(teams) {
			vm.selectedTeamIds = teams;
			$stateParams.selectedTeamIds = vm.selectedTeamIds;
			vm.focusSearch();
			vm.activeSearchIcon();
			vm.selectedFavorite = false;
		};

		vm.toggles = teamsToggles.all();
		vm.onSelectedTeamsInitDefer = $q.defer();
		if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled) {
			vm.onSelectedTeamsInitDefer.resolve();
		}

		vm.onFavoriteSearchInitDefer = $q.defer();

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			vm.selectedTeamIds = currentFavorite.TeamIds;
			vm.searchOptions.keyword = currentFavorite.SearchTerm;
			vm.resetSchedulePage();
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
			defaultTeams: vm.onSelectedTeamsInitDefer.promise,
			defaultFavoriteSearch: vm.onFavoriteSearchInitDefer.promise
		};

		if (!(vm.toggles.WfmTeamSchedule_SaveFavoriteSearchesInWeekView_42576)) {
			vm.onFavoriteSearchInitDefer.resolve();
		}
		if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled) {
			vm.onSelectedTeamsInitDefer.resolve();
		}

		$q.all(asyncData).then(function init(data) {
			if (data.pageSetting.Agents > 0) {
				vm.paginationOptions.pageSize = data.pageSetting.Agents;
			}

			var defaultFavoriteSearch = data.defaultFavoriteSearch;
			var defaultTeams = data.defaultTeams;

			if (!$stateParams.do && defaultFavoriteSearch) {
				vm.selectedTeamIds = defaultFavoriteSearch.TeamIds;
				vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
				vm.selectedFavorite = defaultFavoriteSearch;
			} else if (!$stateParams.do && defaultTeams) {
				vm.selectedTeamIds = defaultTeams;
			}

			vm.resetSchedulePage();
			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
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
			var options = {DomainType: 'IScheduleChangedInDefaultScenario'};
			signalR.subscribeBatchMessage(options, scheduleChangedEventHandler, 300);
		}

		function scheduleChangedEventHandler() {
			$scope.$evalAsync(vm.loadSchedules);
		}

	}
})()