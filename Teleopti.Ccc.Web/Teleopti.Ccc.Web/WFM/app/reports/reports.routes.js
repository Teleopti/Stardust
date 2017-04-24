(function () {
	'use strict';

	angular
		.module('wfm.reports')
		.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('reports/leaderboard',
			{
				url: '/reports/leaderboard',
				templateUrl: 'app/reports/html/leaderboard.html',
				controller: 'LeaderBoardController as vm'
			})
			.state('reports',
			{
				url: '/reports',
				resolve: {
					toggles: function(Toggle) {
						return Toggle;
					}
				},
				controller: function($state, toggles) {
					if (toggles.WFM_ReportHierarchy_43002) {
						$state.go('reports-new');
					} else {
						$state.go('reports-old');
					}
				},
			})
			.state('reports-new',
			{
				templateUrl: 'app/reports/html/reports.overview.html',
				controller: 'ReportsOverviewController as reports'

			})
			.state('reports-old',
			{
				templateUrl: 'app/reports/html/reports.html',
				controller: 'ReportsController as reports'
			});
	}
})();
