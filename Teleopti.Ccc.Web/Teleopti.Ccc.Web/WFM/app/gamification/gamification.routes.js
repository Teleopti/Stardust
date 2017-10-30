(function () {
	'use strict';

	angular
		.module('wfm.gamification')
		.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('gamification',
		{
			url: '/gamification',
			templateUrl: 'app/gamification/html/gamification.html',
			controller: 'GamificationDefaultCtrl'
		});
	}
})();
