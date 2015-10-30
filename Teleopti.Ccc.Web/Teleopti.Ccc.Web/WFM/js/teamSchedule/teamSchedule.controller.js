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

		vm.format = 'yyyy/MM/dd';

		vm.datePickerStatus = {
			opened: false
		};

		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		vm.selectedTeamIdChanged = function() {
			vm.currentPageIndex = 1;
			vm.loadSchedules();
		}

		vm.scheduleDateChanged = function () {
			vm.currentPageIndex = 1;
			vm.loadTeams();
			vm.loadSchedules();
		}

		vm.currentPageIndex = 1;
		vm.totalPages = 0;

		vm.loadTeams = function () {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
			});
		}

		vm.loadSchedules = function() {
			if (vm.selectedTeamId === "") return;

			teamScheduleSvc.loadSchedules.query({
				groupId: vm.selectedTeamId,
				date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: 18,
				currentPageIndex: vm.currentPageIndex
			}).$promise.then(function (result) {
				vm.totalPages = result.TotalPages;
				vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
				vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
			});
		}

		vm.getVisiblePageNumbers = function (start, end) {
			var displayPageCount = 5;
			var ret = [];

			if (!start) {
				start = vm.totalPages;
			}

			if (!end) {
				end = start;
				start = 1;
			}

			var leftBoundary = start;
			var rightBoundary = end;
			if (end - start >= displayPageCount) {
				var index = vm.currentPageIndex;

				if (index < displayPageCount - 1) {
					leftBoundary = 1;
					rightBoundary = displayPageCount;
				} else if (end - index < 3) {
					leftBoundary = end - displayPageCount + 1;
					rightBoundary = end;
				} else {
					leftBoundary = index - Math.floor(displayPageCount / 2) > 1 ? index - Math.floor(displayPageCount / 2) : 1;
					rightBoundary = index + Math.floor(displayPageCount / 2) > end ? end : index + Math.floor(displayPageCount / 2);
				}
			}

			for (var i = leftBoundary; i <= rightBoundary ; i++) {
				ret.push(i);
			}

			return ret;
		};

		vm.gotoPage = function (pageIndex) {
			vm.currentPageIndex = pageIndex;
			vm.loadSchedules();
		}

		vm.Init = function () {
			vm.scheduleDate = new Date();
			vm.loadTeams();
		}

		vm.Init();
	}
}());