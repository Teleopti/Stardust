(function () {
	'use strict';

	angular
	.module('wfm.reports')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('reports', {
			url: '/reports',
			templateUrl: 'js/reports/html/reports.html',
			controller: 'ReportsController as reports'
		}).state('reports/leaderboard', {
			url: '/reports/leaderboard',
			templateUrl: 'js/reports/html/leaderboard.html',
			controller: 'LeaderBoardController as vm'
		})
	}
})();
