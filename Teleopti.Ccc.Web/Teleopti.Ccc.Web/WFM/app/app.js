'use strict';

var externalModules = angular.module('externalModules', ['ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'ngSanitize',
	'pascalprecht.translate',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection',
	'ngStorage',
	'dialogs.main',
	'ui.bootstrap.persian.datepicker'
]);

var wfm = angular.module('wfm', [
	'externalModules',
	'currentUserInfoService',
	'toggleService',
	'shortcutsService',
	'wfm.http',
	'wfm.exceptionHandler',
	'wfm.permissions',
	'wfm.people',
	'wfm.outbound',
	'wfm.forecasting',
	'wfm.resourceplanner',
	'wfm.searching',
	'wfm.seatMap',
	'wfm.skillPrio',
	'wfm.seatPlan',
	'wfm.notifications',
	'wfm.notice',
	'wfm.areas',
	'wfm.help',
	'wfm.rta',
	'wfm.start',
	'wfm.businessunits',
	'wfm.teamSchedule',
	'wfm.intraday',
	'wfm.requests',
	'wfm.themes',
	'wfm.reports',
	'wfm.signalR',
	'wfm.culturalDatepicker',
	'wfm.utilities'
]);

wfm.config([
	'$stateProvider', '$urlRouterProvider', '$translateProvider', '$httpProvider', 'RtaStateProvider',
	function($stateProvider, $urlRouterProvider, $translateProvider, $httpProvider, RtaStateProvider) {

		$urlRouterProvider.otherwise("/#");

		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html'
		});

		RtaStateProvider.config($stateProvider);

		$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
		$translateProvider.useUrlLoader('../api/Global/Language');
		$translateProvider.preferredLanguage('en');
		$httpProvider.interceptors.push('httpInterceptor');
	}
]).run([
	'$rootScope', '$state', '$translate', '$timeout', 'CurrentUserInfo', 'Toggle', '$q', 'RtaState', 'WfmShortcuts', '$locale',
	function($rootScope, $state, $translate, $timeout, currentUserInfo, toggleService, $q, RtaState, WfmShortcuts, $locale) {
		$rootScope.isAuthenticated = false;

		(function broadcastEventOnToggle() {
			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function() {
				$timeout(function() {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});
		})();

		function refreshContext(event, next, toParams) {
			currentUserInfo.initContext().then(function() {
				$rootScope.isAuthenticated = true;
				$translate.fallbackLanguage('en');

				$state.go(next, toParams);

			});
		};

		$rootScope.$on('$localeChangeSuccess', function () {
			if ($locale.id === 'zh-cn')
				$locale.DATETIME_FORMATS.FIRSTDAYOFWEEK = 0;
		});

		$rootScope.$on('$stateChangeStart', function(event, next, toParams) {

			if (!currentUserInfo.isConnected()) {
				event.preventDefault();
				refreshContext(event, next, toParams);
				return;
			}
			if (!toggleService.togglesLoaded.$$state.status) {
				event.preventDefault();
				toggleService.togglesLoaded.then(function() {
					$state.go(next, toParams);
				});
				return;
			}
		});

		RtaState(toggleService);
	}

]);
