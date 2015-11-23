(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory', 'Toggle', TeamScheduleController]);

	function TeamScheduleController(teamScheduleSvc, currentUserInfo, groupScheduleFactory, toggleSvc) {
		var vm = this;

		vm.selectedTeamId = '';
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function() {
			return moment(vm.scheduleDate);
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

		vm.loadSchedules = function (currentPageIndex) {
			if (vm.selectedTeamId === "") return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.loadScheduelWithReadModel) {
				teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: pageSize,
					currentPageIndex: currentPageIndex
				}).$promise.then(function (result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
					vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
				});
			} else {
				if (vm.allAgents == undefined) {
					teamScheduleSvc.loadSchedulesNoReadModel.query({
						groupId: vm.selectedTeamId,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
					    vm.rawScheduleData = result;

						vm.allAgents = [];
						angular.forEach(vm.rawScheduleData, function(schedule) {
							if (vm.allAgents.indexOf(schedule.PersonId) == -1) {
								vm.allAgents.push(schedule.PersonId);
							}
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					    vm.isLoading = false;
					});
				} else {
					getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			}
		}

		vm.Init = function () {
			vm.scheduleDate = new Date();
			toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_NoReadModel_35609' })
				.$promise.then(function (result) {
					vm.loadScheduelWithReadModel = !result.IsEnabled;
				});
			vm.loadTeams();
		}

		vm.Init();
	}
}());