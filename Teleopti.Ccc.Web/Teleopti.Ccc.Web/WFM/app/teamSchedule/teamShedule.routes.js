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
				keyword: undefined,
				selectedDate: undefined,
				selectedTeamIds: undefined,
				selectedFavorite: undefined,
				teamNameMap: undefined,
				do: false,
			}
		}).state('teams.for', {
			url: '/?personId&{selectedDate:date}',
			templateUrl: 'app/teamSchedule/html/dayViewSchedule.html',
			controller: 'TeamScheduleController as vm',
			params: {
				personId: '',
				selectedDate: undefined
			}
		}).state('teams.weekView', {
			url: '/week',
			params: {
				keyword: undefined,
				selectedDate: undefined,
				selectedTeamIds: undefined,
				selectedFavorite: undefined,
				teamNameMap: undefined,
				do: false,
			},
			templateUrl: 'app/teamSchedule/html/weekViewSchedule.html',
			controller: 'TeamScheduleWeeklyController as vm'
		})
	}
})();
