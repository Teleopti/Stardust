(function() {
	'use strict';
	var myteam = angular
		.module('wfm.teamSchedule', [
			'currentUserInfoService',
			'wfm.pagination',
			'toggleService',
			'ngResource',
			'ngSanitize',
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
			'wfm.groupPage',
			'wfm.skillGroup',
			'wfm.throttle',
			'wfm.utilities'
		])
		.config(['$mdThemingProvider', '$mdDateLocaleProvider', teamScheduleConfig])
		.run(['$rootScope', '$state', '$location', 'FavoriteSearchDataService', 'groupPageService', teamScheduleRun]);

	function teamScheduleConfig($mdThemingProvider, $mdDateLocaleProvider) {
		var teleoptiStyleguideWarnMap = $mdThemingProvider.extendPalette('orange', {
			'400': 'FFC285',
			'500': 'FFA726',
			'600': 'FB8C00'
		});
		var teleoptiStyleguidePrimaryMap = $mdThemingProvider.extendPalette('blue', {
			'100': '99D6FF',
			'300': '66C2FF',
			'500': '0099FF'
		});
		$mdThemingProvider.definePalette('teleoptiStyleguideWarn', teleoptiStyleguideWarnMap);
		$mdThemingProvider.definePalette('teleoptiStyleguidePrimary', teleoptiStyleguidePrimaryMap);
		$mdThemingProvider
			.theme('default')
			.primaryPalette('teleoptiStyleguidePrimary')
			.warnPalette('teleoptiStyleguideWarn', {
				default: '400'
			});

		$mdDateLocaleProvider.formatDate = function(date) {
			return moment(date)
				.locale(en)
				.format('YYYY-MM-DD');
		};
	}

	function teamScheduleRun($rootScope, $state, $location, FavoriteSearchDataService, GroupPageService) {
		$rootScope.$on('$stateChangeSuccess', function(event, toState) {
			if (!toState) return;
			if ($location.url() == $state.current.url && toState.name == 'teams') $state.go('teams.dayView');
			if (toState.name.indexOf('teams.') === 0) {
				FavoriteSearchDataService.setModule('wfm.teamSchedule');
				GroupPageService.setModule('wfm.teamSchedule');
			}
		});
	}

	//Polyfill for Object.assign (for IE)
	if (typeof Object.assign != 'function') {
		// Must be writable: true, enumerable: false, configurable: true
		Object.defineProperty(Object, 'assign', {
			value: function assign(target, varArgs) {
				// .length of function is 2
				'use strict';
				if (target == null) {
					// TypeError if undefined or null
					throw new TypeError('Cannot convert undefined or null to object');
				}

				var to = Object(target);

				for (var index = 1; index < arguments.length; index++) {
					var nextSource = arguments[index];

					if (nextSource != null) {
						// Skip over if undefined or null
						for (var nextKey in nextSource) {
							// Avoid bugs when hasOwnProperty is shadowed
							if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
								to[nextKey] = nextSource[nextKey];
							}
						}
					}
				}
				return to;
			},
			writable: true,
			configurable: true
		});
	}
})();
