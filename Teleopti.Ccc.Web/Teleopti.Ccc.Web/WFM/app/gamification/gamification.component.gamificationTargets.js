(function (angular) {

		'use strict';

		angular.module('wfm.gamification')
			.component('gamificationTargets', {
				templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
				controller: GamificationTargetsController
			});

		function GamificationTargetsController() {
			var ctrl = this;
			ctrl.title = 'Set Gamification Targets';
		}

	})(angular);