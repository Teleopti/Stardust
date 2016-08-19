'use strict';
(function () {
	var myteam = angular.module('wfm.teamSchedule', [
			'currentUserInfoService', 'wfm.pagination', 'toggleService', 'ngResource', 'wfmDate', 'wfm.notice', 'ngMaterial', 'ui.bootstrap', 'wfm.signalR', 'ui.router'
		])
		.config([
			'$mdThemingProvider', function($mdThemingProvider) {
				var teleoptiStyleguideWarnMap = $mdThemingProvider.extendPalette('orange', {
					'400': 'FFC285',
					'500': 'FFA726',
					'600': 'FB8C00',
				});
				var teleoptiStyleguidePrimaryMap = $mdThemingProvider.extendPalette('blue', {
					'100': '99D6FF',
					'300': '66C2FF',
					'500': '0099FF',
				});
				$mdThemingProvider.definePalette('teleoptiStyleguideWarn', teleoptiStyleguideWarnMap);
				$mdThemingProvider.definePalette('teleoptiStyleguidePrimary', teleoptiStyleguidePrimaryMap);
				$mdThemingProvider.theme('default')
					.primaryPalette('teleoptiStyleguidePrimary')
					.warnPalette('teleoptiStyleguideWarn', {
						'default': '400'
					});
			}
		]);

	myteam.run([
		'$rootScope', '$state', '$location', function($rootScope, $state, $location) {

			$rootScope.$on('$stateChangeSuccess',
				function(event, toState) {
					if ($location.url() == $state.current.url && toState.name == 'myTeamSchedule') $state.go('myTeamSchedule.dayView');
				});

		}
	]);
})();
