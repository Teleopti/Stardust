(function() {
	'use strict';
	angular.module('wfm.people').config(stateConfig);
	function stateConfig($stateProvider, $urlRouterProvider) {
		$urlRouterProvider.when('/people', '/people/search');
		$stateProvider
			.state('people', {
				url: '/people',
				template: '<div ui-view="content"></div>'
			})
			.state('people.grant', {
				url: '/roles/grant',
				views: {
					content: { template: '<ng2-people-grant-page></ng2-people-grant-page>' }
				}
			})
			.state('people.revoke', {
				url: '/roles/revoke',
				views: {
					content: { template: '<ng2-people-revoke-page></ng2-people-revoke-page>' }
				}
			})
			.state('people.applogon', {
				url: '/access/applicationlogon',
				views: {
					content: { template: '<ng2-people-app-logon-page></ng2-people-app-logon-page>' }
				}
			})
			.state('people.identitylogon', {
				url: '/access/identitylogon',
				views: {
					content: { template: '<ng2-people-identity-logon-page></ng2-people-identity-logon-page>' }
				}
			})
			.state('people.index', {
				url: '/search',
				views: {
					content: { template: '<ng2-people-search-page></ng2-people-search-page>' }
				}
			});
	}
})();
