(function() {
	'use strict';

	angular.module('wfm.reports').config(stateConfig);

	function stateConfig($stateProvider, $urlRouterProvider) {
		$stateProvider
			.state('leaderboardreport', {
				url: '/report/leaderboard',
				templateUrl: 'app/reports/html/leaderboard.html',
				controller: 'LeaderBoardController as vm'
			})
			.state('auditTrailGatekeeper', {
				url: '/report/audit-trail',
				resolve: {
					toggles: function(Toggle) {
						return Toggle;
					}
				},
				controllerAs: 'vm',
				controller: function($state) {
						$state.go('auditTrail');
				}
			})
			.state('reports', {
				url: '/reports',
				templateUrl: 'app/reports/html/reports.overview.html',
				controller: 'ReportsOverviewController as reports'
			})
			.state('auditTrail', {
				url: '/report/audit-trail/',
				templateUrl: 'app/reports/html/audit.html',
				controller: 'AuditTrailController as vm'
			})
			.state('test', {
				url: '/report/test',
				template: '<ng2-general-audit-trail-page></ng2-general-audit-trail-page>'
			});
	}
})();
