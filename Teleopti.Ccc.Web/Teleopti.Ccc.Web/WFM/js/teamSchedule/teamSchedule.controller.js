(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory', TeamScheduleController]);

	function TeamScheduleController(teamScheduleSvc, currentUserInfo, groupScheduleFactory) {
		var vm = this;
		var queryDate = moment(); //
		var promiseForLoadingTeams = teamScheduleSvc.loadAllTeams.query({ date: new Date() }).$promise;
		promiseForLoadingTeams.then(function(result) {
			vm.Teams = result;
		});
		vm.selectedTeamId = '';
		vm.scheduleDate = queryDate;

		vm.selectedTeamChanged = function () {
			vm.canvasSize = angular.element($('#time-line-container'))[0].offsetWidth;
			teamScheduleSvc.loadSchedules.query({ groupId: vm.selectedTeamId, date: queryDate.format("YYYY-MM-DD") }).$promise
				.then(function (data) {
					
					vm.groupScheduleVm = groupScheduleFactory;
					vm.groupScheduleVm.Create(data.Schedules, queryDate, vm.canvasSize);
				});
		}

		
	}
}());