'use strict';

angular.module('externalModules', [
	'ui.router',
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

var wfm = angular
	.module('wfm', [
		'externalModules',
		'currentUserInfoService',
		'toggleService',
		'shortcutsService',
		'wfm.http',
		'wfm.exceptionHandler',
		'wfm.permissions',
		'wfm.peopleold',
		'wfm.outbound',
		'wfm.forecasting',
		'wfm.resourceplanner',
		'wfm.searching',
		'wfm.seatMap',
		'wfm.seatPlan',
		'wfm.notifications',
		'wfm.notice',
		'wfm.areas',
		'wfm.help',
		'wfm.rta',
		'wfm.start',
		'wfm.teamSchedule',
		'wfm.intraday',
		'wfm.requests',
		'wfm.reports',
		'wfm.datePicker',
		'wfm.dateRangePicker',
		'wfm.utilities',
		'wfm.templates',
		'wfm.gamification'
	])
	.config([
		'$stateProvider',
		'$urlRouterProvider',
		'$translateProvider',
		'$httpProvider',
		function($stateProvider, $urlRouterProvider, $translateProvider, $httpProvider) {
			$urlRouterProvider.otherwise('/#');

			$stateProvider.state('main', {
				url: '/',
				templateUrl: 'html/main.html'
			});

			$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
			$translateProvider.useUrlLoader('../api/Global/Language');
			$translateProvider.preferredLanguage('en');
			$httpProvider.interceptors.push('httpInterceptor');
		}
	])
	.run([
		'$rootScope',
		'$state',
		'$translate',
		'$timeout',
		'$locale',
		'CurrentUserInfo',
		'Toggle',
		function($rootScope, $state, $translate, $timeout, $locale, currentUserInfo, toggleService) {
			$rootScope.isAuthenticated = false;

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

			function refreshContext(event, next, toParams) {
				currentUserInfo.initContext().then(function(data) {
					$rootScope.isAuthenticated = true;
					$translate.use(data.Language).then(function() {
						$state.go(next, toParams);
					});

					$rootScope.$on('$localeChangeSuccess', function() {
						if ($locale.id === 'zh-cn') $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK = 0;
					});
				});
			}
		}
	]);
