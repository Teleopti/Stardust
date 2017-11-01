(function (angular) {
	'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargetRow', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargetRow.tpl.html',
			bindings: {
				row: '<',
				selected: '<',
				onSelect: '&',
				onSettingChange: '&'
			},
			controller: [GamificationTargetRowController]
		});

	function GamificationTargetRowController() {}

})(angular);