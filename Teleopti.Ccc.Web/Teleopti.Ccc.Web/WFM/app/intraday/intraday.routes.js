(function () {
	'use strict';

	angular
	.module('wfm.intraday')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('intraday',
		{
			params: {
				isNewSkillArea: false
			},
			url: '/intraday',
			templateUrl: 'app/intraday/intraday.html',
			controller: 'IntradayCtrl'
		})
		.state('intraday.area',
		{
			templateUrl: 'app/intraday/intraday-area.html',
			controller: 'IntradayAreaCtrl'
		})
		.state('intraday.config',
		{
			url: '/config',
			templateUrl: 'app/intraday/intraday-config-new.html',
			controller: 'IntradayConfigCtrl'
		})
	}
})();
