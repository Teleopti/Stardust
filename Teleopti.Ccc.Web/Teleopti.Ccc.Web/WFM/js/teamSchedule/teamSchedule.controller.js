(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$q', 'TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory', 'Toggle', TeamScheduleController]);

	function TeamScheduleController($q, teamScheduleSvc, currentUserInfo, groupScheduleFactory, toggleSvc) {
		var vm = this;

		vm.selectedTeamId = '';
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function() {
			return moment(vm.scheduleDate);
		}
		vm.searchOptions = {
			keyword: "",
			isAdvancedSearchEnabled: false,
			searchKeywordChanged:false
		}

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		var pageSize = 18;
		vm.paginationOptions = { pageNumber: 1, totalPages: 0 };
		vm.format = 'yyyy/MM/dd';

		vm.datePickerStatus = {
			opened: false
		};
		vm.loadScheduelWithReadModel = true;
		vm.isSearchScheduleEnabled = false;
		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		vm.selectedTeamIdChanged = function () {
			vm.allAgents = undefined;
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.scheduleDateChanged = function () {
			vm.loadTeams();
			vm.allAgents = undefined;
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.isLoading = false;
		vm.loadTeams = function () {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
			});
		}

		function getScheduleForCurrentPage(rawScheduleData, currentPageIndex) {
			var start = (currentPageIndex - 1) * pageSize;
			var end = currentPageIndex * pageSize;
			var agentsForCurrentPage = vm.allAgents.slice(start, end);

			var scheduleForCurrentPage = [];
			angular.forEach(rawScheduleData, function(rawSchedule) {
				if (agentsForCurrentPage.indexOf(rawSchedule.PersonId) > -1) {
					scheduleForCurrentPage.push(rawSchedule);
				}
			});

			vm.groupScheduleVm = groupScheduleFactory.Create(scheduleForCurrentPage, vm.scheduleDateMoment());
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
		}

		vm.loadSchedules = function(currentPageIndex) {
			if (vm.selectedTeamId === "" && !vm.isSearchScheduleEnabled) return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.loadScheduelWithReadModel && !vm.isSearchScheduleEnabled) {
				teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: pageSize,
					currentPageIndex: currentPageIndex
				}).$promise.then(function(result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
					vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
				});
			} else if (!vm.isSearchScheduleEnabled) {
				if (vm.allAgents == undefined) {
					teamScheduleSvc.loadSchedulesNoReadModel.query({
						groupId: vm.selectedTeamId,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
						vm.rawScheduleData = result;

						vm.allAgents = [];
						var allSchedules = groupScheduleFactory.Create(result, vm.scheduleDateMoment()).Schedules; // keep the agents in right order
						angular.forEach(allSchedules, function(schedule) {
							vm.allAgents.push(schedule.PersonId);
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
						
						vm.isLoading = false;
					});
				} else {
					getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			} else if (vm.isSearchScheduleEnabled) {
				vm.paginationOptions.pageNumber = vm.searchOptions.searchKeywordChanged ? 1 : currentPageIndex;
				if (vm.allAgents == undefined || vm.searchOptions.searchKeywordChanged) {
					teamScheduleSvc.searchSchedules.query({
						keyword: vm.searchOptions.keyword,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
						vm.rawScheduleData = result.Schedules;
						vm.total = result.Total;
						if (vm.searchOptions.keyword == "" && result.Keyword != "") {
							vm.searchOptions.keyword = result.Keyword;
						}

						vm.allAgents = [];
						var allSchedules = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment()).Schedules; // keep the agents in right order
						angular.forEach(allSchedules, function(schedule) {
							vm.allAgents.push(schedule.PersonId);
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
						vm.searchOptions.searchKeywordChanged = false;
						vm.isLoading = false;
					});
				} else {
					getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			}
		}
		vm.searchSchedules = function() {
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}
		var loadWithoutReadModelTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_NoReadModel_35609' }).$promise;
		loadWithoutReadModelTogglePromise.then(function (result) {
			vm.loadScheduelWithReadModel = !result.IsEnabled;
		});

		var advancedSearchTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdvancedSearch_32973' }).$promise;
		advancedSearchTogglePromise.then(function (result) {
			vm.searchOptions.isAdvancedSearchEnabled = result.IsEnabled;
		});

		var searchScheduleTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_FindScheduleEasily_35611' }).$promise;
		searchScheduleTogglePromise.then(function (result) {
			vm.isSearchScheduleEnabled = result.IsEnabled;
		});

		vm.Init = function () {
			vm.loadTeams();

			$q.all([loadWithoutReadModelTogglePromise, advancedSearchTogglePromise, searchScheduleTogglePromise]).then(function () {
				if (vm.isSearchScheduleEnabled) {
					vm.searchSchedules();
				}
			});
		}

		vm.Init();
	}
}());