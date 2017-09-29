(function () {
	'use strict';

	angular
	.module('wfm.reports')
	.config(stateConfig);

	function stateConfig($stateProvider, $urlRouterProvider) {
		$stateProvider.state('leaderboardreport', {
			url: '/report/leaderboard',
			templateUrl: 'app/reports/html/leaderboard.html',
			controller: 'LeaderBoardController as vm'
		})
		.state('auditTrailGatekeeper', {
			url: '/report/audit-trail',
			resolve: {
				toggles: function (Toggle) {
					return Toggle;
				}
			},
			controller: function ($state, toggles) {
				if (toggles.WFM_AuditTrail_44006) {
					$state.go('auditTrail');
				} else {
					$state.go('main');
				}
			}
		})
		.state('reports', {
			url: '/reports',
			templateUrl: 'app/reports/html/reports.overview.html',
			controller: 'ReportsOverviewController as reports'
		})
		.state('auditTrail', {
			url: '/report/audit-trail',
			templateUrl: 'app/reports/html/audit.html',
			controller: 'AuditTrailController as vm'
		});
	}
})();
