(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamification', {
			templateUrl: 'app/gamification/html/g.component.gamification.tpl.html',
			controller: GamificationController
		});

	function GamificationController() {}

})(angular);