(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', TeamScheduleController]);

	function TeamScheduleController(teamScheduleSvc) {
		var vm = this;
		var promiseForLoadingTeams = teamScheduleSvc.loadAllTeams.query({ date: new Date() }).$promise;
		promiseForLoadingTeams.then(function(result) {
			vm.Teams = result;
		});
	}
}());