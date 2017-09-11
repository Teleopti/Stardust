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
			controller: 'IntradayController'
		})
		.state('intraday.area',
		{
			templateUrl: 'app/intraday/intraday-area.html',
			controller: 'IntradayAreaController'
		})
		.state('intraday.skill-area-config',
		{
			url: '/skill-area-config',
			templateUrl: 'app/intraday/intraday-config-new.html',
			controller: 'IntradayConfigController'
		})
	}
})();
