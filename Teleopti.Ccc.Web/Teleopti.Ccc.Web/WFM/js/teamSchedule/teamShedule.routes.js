(function () {
	'use strict';

	angular
	.module('wfm.teamSchedule')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('myTeamSchedule', {
			url: '/myTeam',
			templateUrl: 'js/teamSchedule/html/schedule.html',
			controller: 'TeamScheduleDefaultCtrl as vm'
		}).state('myTeamSchedule.dayView', {
			templateUrl: 'js/teamSchedule/html/dayViewSchedule.html',
			controller: 'TeamScheduleCtrl as vm',
			params: {
				keyword: '',
				selectedDate: new Date()
			}
		}).state('myTeamSchedule.for', {
			url: '/?personId',
			templateUrl: 'js/teamSchedule/html/dayViewSchedule.html',
			controller: 'TeamScheduleCtrl as vm',
			params: {
				personId: ''
			}
		}).state('myTeamSchedule.weekView', {
			url: '/week',
			params: {
				keyword: '',
				selectedDate: new Date()
			},
			templateUrl: 'js/teamSchedule/html/weekViewSchedule.html',
			controller: 'TeamScheduleWeeklyCtrl as vm'
		})
	}
})();
