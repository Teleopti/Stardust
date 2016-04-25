'use strict';
(function () {
	angular.module('wfm.teamSchedule', [
		'currentUserInfoService', 'wfm.pagination', 'toggleService', 'ngResource', 'wfmDate', 'wfm.notice', 'ngMaterial'])
		.config(['$mdThemingProvider', function ($mdThemingProvider) {
			var teleoptiStyleguideWarnMap = $mdThemingProvider.extendPalette('orange', {
				'400': 'FFC285',
				'500': 'FFA726',
				'600': 'FB8C00',
			});
			$mdThemingProvider.definePalette('teleoptiStyleguideWarn', teleoptiStyleguideWarnMap);
			$mdThemingProvider.theme('default')
				.warnPalette('teleoptiStyleguideWarn', {
					'default': '400'
				});
		}]);
})();
