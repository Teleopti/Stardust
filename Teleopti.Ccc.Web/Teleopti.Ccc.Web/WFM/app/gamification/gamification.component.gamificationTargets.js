(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargets', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
			controller: GamificationTargetsController
		});

	function GamificationTargetsController() {
		var ctrl = this;
		ctrl.sites = [
			{ id: 10, name: 'Site A' },
			{ id: 12, name: 'Site B' }
		];
	}

})(angular);