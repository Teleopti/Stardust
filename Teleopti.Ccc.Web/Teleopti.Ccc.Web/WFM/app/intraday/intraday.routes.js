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
				resolve: {
					toggles: function (Toggle) {
						return Toggle;
					}
				},
				controller: function ($state, toggles) {
					if (toggles.WFM_ImproveIntradaySkillArea_37291) {
						$state.go('intraday-config-new');
					} else {
						$state.go('intraday-config-old');
					}
				},
			})
			.state('intraday-config-new', {
				templateUrl: 'app/intraday/intraday-config-new.html',
				controller: 'IntradayConfigCtrl'

			}).state('intraday-config-old', {
				templateUrl: 'app/intraday/intraday-config.html',
				controller: 'IntradayConfigCtrl'
			})
	}
})();
