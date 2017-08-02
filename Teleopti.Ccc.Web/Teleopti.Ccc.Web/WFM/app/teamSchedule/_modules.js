(function() {
	'use strict';
	var myteam = angular.module('wfm.teamSchedule', [
			'currentUserInfoService',
			'wfm.pagination',
			'toggleService',
			'ngResource',
			'wfm.notice',
			'ngMaterial',
			'ui.bootstrap',
			'wfm.signalR',
			'ui.router',
			'wfm.multiplesearchinput',
			'wfm.utilities',
			'wfm.confirmModal',
			'wfm.favoriteSearch',
			'wfm.organizationPicker',
			'wfm.focusInput',
			'wfm.groupPage'
	])
	.config(['$mdThemingProvider', teamScheduleConfig])
		.run(['$rootScope', '$state', '$location', 'FavoriteSearchDataService', teamScheduleRun]);

	function teamScheduleConfig($mdThemingProvider) {
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

	function teamScheduleRun($rootScope, $state, $location, FavoriteSearchDataService) {
		$rootScope.$on('$stateChangeSuccess',
			function (event, toState) {
				if (!toState) return;
				if ($location.url() == $state.current.url && toState.name == 'teams') $state.go('teams.dayView');
				if (toState.name.indexOf("teams.") === 0) {
					FavoriteSearchDataService.setModule("wfm.teamSchedule");
				}
			});
	}
})();