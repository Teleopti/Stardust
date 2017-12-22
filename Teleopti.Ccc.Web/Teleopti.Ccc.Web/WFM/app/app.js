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
		'areasService',
		'NoticeService',
		function(
			$rootScope,
			$state,
			$translate,
			$timeout,
			$locale,
			currentUserInfo,
			toggleService,
			RtaState,
			areasService,
			noticeService
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
			setupPermissionCheckForModules();

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

			function setupPermissionCheckForModules() {
				areasService.getAreasWithPermission().then(function(areasWithPermission) {
					areasService.getAreasList().then(function(areasList) {
						$rootScope.$on('$stateChangeStart', function(event, next, toParams) {
							var areaName, moduleName,
								hasModulePermission = false,
								name = next.name.split('.')[0],
								url = next.url && next.url.split('/')[1];

							areasWithPermission.forEach(function(area) {
								if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
									hasModulePermission = true;
								} else if (url && (area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)) {
									hasModulePermission = true;
								}
							});

							if (!hasModulePermission && name != 'main') {
								event.preventDefault();
								$state.go('main');

								areasList.forEach(function(area) {
									if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
										moduleName = area.Name;
									} else if (url && (area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)) {
										moduleName = area.Name;
									}
								});

								noticeService.error("<span class='test-alert'></span>" + $translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', moduleName), null, false);
							}
						});
					});
				});
			}
		}
	])
	.constant('_', window._);