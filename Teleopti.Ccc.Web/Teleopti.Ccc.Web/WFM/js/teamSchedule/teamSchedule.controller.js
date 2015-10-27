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

		vm.scheduleDateChanged = function () {
			vm.loadTeams();
			vm.loadSchedules();
		}

		vm.loadTeams = function() {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function(result) {
				vm.Teams = result;
			});
		}

		vm.loadSchedules = function () {
			if (vm.selectedTeamId === "") return;

			teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD")
				}).$promise
				.then(function(data) {
					vm.groupScheduleVm = groupScheduleFactory;
					vm.groupScheduleVm.Create(data, vm.scheduleDateMoment());
				});
		}

		vm.Init = function () {
			vm.scheduleDate = new Date();
			vm.loadTeams();
		}

		vm.Init();
	}
}());