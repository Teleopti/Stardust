(function () {
	'use strict';

	angular
	.module('wfm.requests')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('requests', {
			url: '/requests',
			templateUrl: 'app/requests/html/requests.html',
			controller: 'RequestsCtrl as requests'
		})
	}
})();
