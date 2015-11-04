(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory', TeamScheduleController]);

	function TeamScheduleController(teamScheduleSvc, currentUserInfo, groupScheduleFactory) {
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
		vm.paginationOptions = { pageNumber: 1, totalPages: 0 };
		vm.format = 'yyyy/MM/dd';

		vm.datePickerStatus = {
			opened: false
		};

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

		

		vm.loadTeams = function () {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
			});
		}

		vm.loadSchedules = function (currentPageIndex) {
			if (vm.selectedTeamId === "") return;
			vm.paginationOptions.pageNumber = currentPageIndex;
			teamScheduleSvc.loadSchedules.query({
				groupId: vm.selectedTeamId,
				date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: 18,
				currentPageIndex: currentPageIndex
			}).$promise.then(function (result) {
				vm.paginationOptions.totalPages = result.TotalPages;
				vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
				vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
			});
		}

		vm.Init = function () {
			vm.scheduleDate = new Date();
			vm.loadTeams();
		}

		vm.Init();
	}
}());