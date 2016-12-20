(function () {
	'use strict';

	angular.module('wfm.teamSchedule').config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('teams', {
			url: '/teams',
			templateUrl: 'app/teamSchedule/html/schedule.html',
			controller: 'TeamScheduleDefaultController as vm'
		}).state('teams.dayView', {
			templateUrl: 'app/teamSchedule/html/dayViewSchedule.html',
			controller: 'TeamScheduleController as vm',
			params: {
				keyword: '',
				selectedDate: new Date()
			}
		}).state('teams.for', {
			url: '/?personId',
			templateUrl: 'app/teamSchedule/html/dayViewSchedule.html',
			controller: 'TeamScheduleController as vm',
			params: {
				personId: ''
			}
		}).state('teams.weekView', {
			url: '/week',
			params: {
				keyword: '',
				selectedDate: new Date()
			},
			templateUrl: 'app/teamSchedule/html/weekViewSchedule.html',
			controller: 'TeamScheduleWeeklyController as vm'
		})
	}
})();
