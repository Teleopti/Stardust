(function() {
	'use strict';
	angular.module('wfm.apiaccess').config(stateConfig);
	function stateConfig($stateProvider, $urlRouterProvider) {
		$urlRouterProvider.when('/api-access', '/api-access/list');
		$stateProvider
			.state('apiaccess', {
				url: '/api-access',
				template: '<div ui-view="content"></div>'
			})
			.state('apiaccess.index', {
				url: '/list',
				views: {
					content: { template: '<ng2-api-access-list-page></ng2-api-access-list-page>' }
				}
			})
			.state('apiaccess.addapp', {
				url: '/add-app',
				views: {
					content: { template: '<ng2-api-access-add-app-page></ng2-api-access-add-app-page>' }
				}
			});
	}
})();
