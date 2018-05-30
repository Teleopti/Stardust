(function () {
	'use strict';

	angular.module('wfm.teamSchedule').config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('teams',
			{
				url: '/teams',
				templateUrl: 'app/teamSchedule/html/schedule.html',
				controller: 'TeamScheduleDefaultController as vm'
			}).state('teams.dayView',
			{
				templateUrl: 'app/teamSchedule/html/dayViewSchedule.html',
				controller: 'TeamScheduleController as vm',
				params: {
					keyword: undefined,
					selectedDate: undefined,
					selectedTeamIds: undefined,
					selectedGroupPage: undefined,
					selectedFavorite: undefined,
					selectedSortOption: undefined,
					teamNameMap: undefined,
					staffingEnabled: false,
					do: false
				}
			}).state('teams.for',
			{
				url: '/?personId&{selectedDate:date}',
				templateUrl: 'app/teamSchedule/html/dayViewSchedule.html',
				controller: 'TeamScheduleController as vm',
				params: {
					personId: '',
					selectedDate: undefined
				}
			}).state('teams.weekView',
			{
				url: '/week',
				params: {
					keyword: undefined,
					selectedDate: undefined,
					selectedTeamIds: undefined,
					selectedGroupPage: undefined,
					selectedFavorite: undefined,
					selectedSortOption: undefined,
					teamNameMap: undefined,
					staffingEnabled: false,
					do: false
				},
				templateUrl: 'app/teamSchedule/html/weekViewSchedule.html',
				controller: 'TeamScheduleWeeklyController as vm'
			}).state('teams.exportSchedule',
			{
				url: '/exportSchedule',
				templateUrl: 'app/teamSchedule/html/exportScheduleView.html'
			}).state('teams.shiftEditor',
			{
				url: '/shiftEditor?:personId&:date&:timezone',
				controller:'ShiftEditorViewController as vm',
				templateUrl: 'app/teamSchedule/html/shiftEditorView.html',
				params: {
					personId: '',
					date: '',
					timezone: ''
				}
			});
	}
})();
