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
	'wfm.peopleold',
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
	'wfm.teapot',
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
	'$mdGestureProvider',
	function ($stateProvider,
			  $urlRouterProvider,
			  $translateProvider,
			  $httpProvider,
			  $mdGestureProvider) {
		$urlRouterProvider.otherwise('/#');

		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html'
		});

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
		'areasService',
		'NoticeService',
		'$q',
		function ($rootScope,
				  $state,
				  $translate,
				  $timeout,
				  $locale,
				  currentUserInfo,
				  toggleService,
				  areasService,
				  noticeService,
				  $q) {
			$rootScope.isAuthenticated = false;

			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function () {
				$timeout(function () {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});

			$rootScope.$on('$localeChangeSuccess', function () {
				if ($locale.id === 'zh-cn') $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK = 0;
			});

			var preloads = [];
			preloads.push(toggleService.togglesLoaded);
			preloads.push(initializeUserInfo());
			preloads.push(initializePermissionCheck());
			var preloadDone = false;

			$rootScope.$on('$stateChangeStart', function (event, next, toParams) {
				if (preloadDone) {
					if (!permitted(event, next)) {
						event.preventDefault();
						notPermitted();
					}
					return;
				}
				preloadDone = true;
				event.preventDefault();
				$q.all(preloads).then(function () {
					$state.go(next, toParams);
				})
			});

			$rootScope._ = window._;

			function initializeUserInfo() {
				return currentUserInfo.initContext().then(function (data) {
					$rootScope.isAuthenticated = true;
					return $translate.use(data.Language);
				});
			}

			var areas;
			var permittedAreas;
			var alwaysPermittedAreas = [
				'main',
				'skillprio',
				'teapot',
				'resourceplanner.importschedule',
				'resourceplanner.archiveschedule'
			];

			function initializePermissionCheck() {
				return areasService.getAreasWithPermission().then(function (data) {
					permittedAreas = data;
					return areasService.getAreasList().then(function (data) {
						areas = data;
					});
				});
			}

			function permitted(event, next) {
				var name = next.name.split('.')[0];
				var url = next.url && next.url.split('/')[1];

				var permitted = alwaysPermittedAreas.some(function (a) {
					return a === next.name.toLowerCase();
				});

				if (!permitted)
					permittedAreas.forEach(function (area) {
						if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
							permitted = true;
						} else if (url && (area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)) {
							permitted = true;
						}
					});

				return permitted;
			};

			function notPermitted() {
				$state.go('main');

				var moduleName;
				areas.forEach(function (area) {
					if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
						moduleName = area.Name;
					} else if (url && (area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)) {
						moduleName = area.Name;
					}
				});

				noticeService.error("<span class='test-alert'></span>" + $translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', moduleName), null, false);
			}

		}
	])
	.constant('_', window._);
