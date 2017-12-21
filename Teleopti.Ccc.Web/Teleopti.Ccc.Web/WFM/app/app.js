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
		'wfm.rtaShared',
		'wfm.rta',
		'wfm.rtaTool',
		'wfm.rtaTracer',
		'wfm.start',
		'wfm.businessunits',
		'wfm.teamSchedule',
		'wfm.intraday',
		'wfm.requests',
		'wfm.themes',
		'wfm.reports',
		'wfm.signalR',
		'wfm.culturalDatepicker',
		'wfm.utilities',
		'wfm.staffing',
		'wfm.dataProtection',
		'wfm.templates',
		'wfm.workPicker',
		'wfm.badge',
		'wfm.skillPicker',
		'wfm.treePicker',
		'wfm.card-panel',
		'wfm.skillGroup',
		'wfm.calendarPicker',
		'wfm.popup',
		'wfm.gamification',
		'wfm.btnGroup'
	]).config([
		'$stateProvider',
		'$urlRouterProvider',
		'$translateProvider',
		'$httpProvider',
		'RtaStateProvider',
		'$mdGestureProvider',
		function(
			$stateProvider,
			$urlRouterProvider,
			$translateProvider,
			$httpProvider,
			RtaStateProvider,
			$mdGestureProvider
		) {
			$urlRouterProvider.otherwise('/#');

			$stateProvider.state('main', {
				url: '/',
				templateUrl: 'html/main.html'
			});

			RtaStateProvider.config($stateProvider);

			$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
			$translateProvider.useUrlLoader('../api/Global/Language');

			$translateProvider.preferredLanguage('en');
			$httpProvider.interceptors.push('httpInterceptor');
			$mdGestureProvider.skipClickHijack();
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
		'RtaState',
		function(
			$rootScope,
			$state,
			$translate,
			$timeout,
			$locale,
			currentUserInfo,
			toggleService,
			RtaState,
		) {
			$rootScope.isAuthenticated = false;

			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function() {
				$timeout(function() {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});

			$rootScope.$on('$localeChangeSuccess', function() {
				if ($locale.id === 'zh-cn') $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK = 0;
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

			$rootScope._ = window._;

			RtaState(toggleService);

			function refreshContext(event, next, toParams) {
				var localLang = '';
				currentUserInfo.initContext().then(function(data) {
					$rootScope.isAuthenticated = true;
					$translate.use(data.Language).then(function() {
						$state.go(next, toParams);
					});
				});
			}
		}
	])
	.constant('_', window._);