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
			}).state('gamification.setting', {
				url: '.setting',
				templateUrl: 'app/gamification/html/gamification.html',
				controller: 'GamificationDefaultCtrl'
			}).state('gamification.targets', {
				url: '.targets',
				templateUrl: 'app/gamification/html/gamification.html',
				controller: 'GamificationDefaultCtrl'
			}).state('gamification.import', {
				url: '.import',
				templateUrl: 'app/gamification/html/gamification.html',
				controller: 'GamificationDefaultCtrl'
			});
	}
})();
