(function () {
	'use strict';

	angular
	.module('wfm.intraday')
	.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('intraday', {
			params: {
				isNewSkillArea: false
			},
			url: '/intraday',
			templateUrl: 'js/intraday/intraday.html',
			controller: 'IntradayCtrl'
		}).state('intraday.area', {
			templateUrl: 'js/intraday/intraday-area.html',
			controller: 'IntradayAreaCtrl'
		}).state('intraday.config', {
			url: '/config',
			templateUrl: 'js/intraday/intraday-config.html',
			controller: 'IntradayConfigCtrl'
		})
	}
})();
