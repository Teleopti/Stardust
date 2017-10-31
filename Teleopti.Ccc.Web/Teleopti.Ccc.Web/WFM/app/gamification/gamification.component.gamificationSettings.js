(function (angular) {

	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationSettings', {
			templateUrl: 'app/gamification/html/g.component.gamificationSettings.tpl.html',
			controller: GamificationSettingsController
		});

	function GamificationSettingsController() {
		var ctrl = this;
		ctrl.title = 'Gamification Settings';
	}

})(angular);