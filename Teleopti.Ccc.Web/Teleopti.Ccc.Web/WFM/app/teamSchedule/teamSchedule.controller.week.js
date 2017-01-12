(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyController', TeamScheduleWeeklyController);

	TeamScheduleWeeklyController.$inject = ['$stateParams', '$q','$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc', 'TeamSchedule', 'signalRSVC', '$scope', 'teamsToggles'];

	function TeamScheduleWeeklyController(params, $q, $locale, $filter, WeekViewCreator, Util, weekViewScheduleSvc, teamScheduleSvc, signalR, $scope, teamsToggles) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined(params.keyword) && params.keyword !== '' ? params.keyword : '',
			searchKeywordChanged: false,
			searchFields : [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBags',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};

		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];
		vm.scheduleDate = params.selectedDate || new Date();
		vm.selectedTeamIds = params.selectedTeamIds || [];
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };

		vm.startOfWeek = moment(vm.scheduleDate).startOf('week').toDate();

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadSchedules();
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
		};

		vm.changeSelectedTeams = function(teams) {
			vm.selectedTeamIds = teams;
			params.selectedTeamIds = vm.selectedTeamIds;
			vm.resetSchedulePage();
		};

		vm.init = function () {
			vm.toggles = teamsToggles.all();		
			vm.weekDays = Util.getWeekdays(vm.scheduleDate);
			vm.paginationOptions.totalPages = 1;

			if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled)
				vm.loadSchedules();

			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
		};

		$q.all([
			teamScheduleSvc.getAvailableHierarchy(vm.scheduleDateMoment().format("YYYY-MM-DD"))
				.then(function (response) {
					var data = response.data;
					vm.selectedTeamIds = vm.selectedTeamIds.length > 0 ? vm.selectedTeamIds : [data.LogonUserTeamId];

					vm.availableGroups = {
						sites: data.Children,
					};
			})
			]).then(vm.init);

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