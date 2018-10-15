(function() {
	'use strict';
	angular.module('wfm.pm').config(stateConfig);
	function stateConfig($stateProvider, $urlRouterProvider) {
		$urlRouterProvider.when('/pm', '/pm/workspace');
		$stateProvider
			.state('pm', {
				url: '/pm',
				template: '<div ui-view="content"></div>',
			})
			.state('pm.workspace', {
				url: '/workspace',
				views: {
					content: { template: '<ng2-pm-workspace-page></ng2-pm-workspace-page>' }
				}
			});
	}
})();
