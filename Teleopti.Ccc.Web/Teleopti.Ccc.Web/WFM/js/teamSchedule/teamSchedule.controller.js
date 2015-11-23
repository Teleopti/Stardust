(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory','Toggle', TeamScheduleController]);

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
		vm.isFromReadModel = true;
		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		vm.selectedTeamIdChanged = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.scheduleDateChanged = function () {
			vm.loadTeams();
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

		function getScheduleForCurrentPage(currentPageIndex) {
			vm.groupScheduleVm = {
				TimeLine: vm.allSchedules.TimeLine,
				Schedules: vm.allSchedules.Schedules.slice((currentPageIndex - 1) * pageSize, currentPageIndex * pageSize)
			}
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
		}

		vm.loadSchedules = function (currentPageIndex) {
			if (vm.selectedTeamId === "") return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.isFromReadModel) {
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
				if (vm.allSchedules == undefined) {
					teamScheduleSvc.loadSchedulesNoReadModel.query({
						groupId: vm.selectedTeamId,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
					vm.isLoading = false;
						vm.allSchedules = groupScheduleFactory.Create(result, vm.scheduleDateMoment());
						vm.paginationOptions.totalPages = Math.ceil(vm.allSchedules.Schedules.length / pageSize);

						getScheduleForCurrentPage(currentPageIndex);
					});
				} else {
					getScheduleForCurrentPage(currentPageIndex);
				}
			}
		}

		vm.Init = function () {
			vm.scheduleDate = new Date();
			toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_NoReadModel_35609' }).$promise.then(function(result) {
				vm.isFromReadModel = !result;
			});
			vm.loadTeams();
		}

		vm.Init();
	}
}());