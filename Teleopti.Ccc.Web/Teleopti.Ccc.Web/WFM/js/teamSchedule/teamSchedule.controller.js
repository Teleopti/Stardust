(function() {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['TeamSchedule', TeamScheduleController]);

	function TeamScheduleController(teamScheduleSvc) {
		var vm = this;
		var promiseForLoadingGroupPages = teamScheduleSvc.loadAailableGroupPages.query({ date: new Date() }).$promise;
		promiseForLoadingGroupPages.then(function(result) {
			vm.GroupPages = result.GroupPages;
			vm.DefaultGroupId = result.DefaultGroupId;
		});
	}
}());