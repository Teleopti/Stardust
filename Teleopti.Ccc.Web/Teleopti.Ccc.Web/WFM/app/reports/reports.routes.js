(function () {
	'use strict';

	angular
		.module('wfm.reports')
		.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('leaderboardreport',
				{
					url: '/report/leaderboard',
					templateUrl: 'app/reports/html/leaderboard.html',
					controller: 'LeaderBoardController as vm'
				})
			.state('reports',
				{
					url: '/reports',
					templateUrl: 'app/reports/html/reports.overview.html',
					controller: 'ReportsOverviewController as reports'
				});
	}
})();
