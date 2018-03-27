import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { downgradeModule, downgradeComponent } from '@angular/upgrade/static';

import { environment } from './environments/environment';
import { IRootScopeService, IControllerConstructor } from 'angular';

import { MainController } from './main.controller';
import {
	SearchPageComponent,
	GrantPageComponent,
	RevokePageComponent,
	AppLogonPageComponent
} from './app/people/components';
import { TitleBarComponent } from './app/people/components/title-bar/title-bar.component';
import { AppModule } from './app/app.module';

export interface IWfmRootScopeService extends IRootScopeService {
	_: any;
	isAuthenticated: boolean;
	setTheme(theme: string): any;
	version: any;
}

if (environment.production) {
	enableProdMode();
}

const bootstrapFnAngularApp = (extraProviders: StaticProvider[]) =>
	platformBrowserDynamic(extraProviders).bootstrapModule(AppModule);

const wfm = angular.module('wfm', [
	downgradeModule(bootstrapFnAngularApp),
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
]);

wfm.controller('MainController', MainController as IControllerConstructor);

wfm.directive('ng2PeopleTitleBar', downgradeComponent({ component: TitleBarComponent }) as angular.IDirectiveFactory);
wfm.directive('ng2PeopleSearchPage', downgradeComponent({
	component: SearchPageComponent
}) as angular.IDirectiveFactory);
wfm.directive('ng2PeopleGrantPage', downgradeComponent({ component: GrantPageComponent }) as angular.IDirectiveFactory);
wfm.directive('ng2PeopleRevokePage', downgradeComponent({
	component: RevokePageComponent
}) as angular.IDirectiveFactory);
wfm.directive('ng2PeopleAppLogonPage', downgradeComponent({
	component: AppLogonPageComponent
}) as angular.IDirectiveFactory);

wfm
	.config([
		'$stateProvider',
		'$urlRouterProvider',
		'$translateProvider',
		'$httpProvider',
		'$mdGestureProvider',
		'tmhDynamicLocaleProvider',
		function(
			$stateProvider,
			$urlRouterProvider,
			$translateProvider,
			$httpProvider,
			$mdGestureProvider,
			tmhDynamicLocaleProvider
		) {
			$urlRouterProvider.otherwise('/#');
			
			$stateProvider.state('main', {
				url: '/',
				templateUrl: '../html/main.html'
			});
			
			$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
			$translateProvider.useUrlLoader('../api/Global/Language');

			$translateProvider.preferredLanguage('en');
			$httpProvider.interceptors.push('httpInterceptor');
			$mdGestureProvider.skipClickHijack();

			tmhDynamicLocaleProvider.localeLocationPattern('node_modules/angular-i18n/angular-locale_{{locale}}.js');
			//	tmhDynamicLocaleProvider.defaultLocale("en-gb");  -- causes problems with unit tests due to reinit of scope
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
		'TabShortCut',
		'$q',
		'$http',
		function(
			$rootScope: IWfmRootScopeService,
			$state,
			$translate,
			$timeout,
			$locale,
			currentUserInfo,
			toggleService,
			areasService,
			noticeService,
			TabShortCut,
			$q,
			$http
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

			var preloads = [];
			preloads.push(toggleService.togglesLoaded);
			preloads.push(initializeUserInfo());
			preloads.push(initializePermissionCheck());
			preloads.push(
				$http.get('../api/Global/Version').then(function(response) {
					if (response.data) {
						// remove if statement with toggle RTA_ReloadUIOnSystemVersionChanged_48196
						$rootScope.version = response.data;
						$http.defaults.headers.common['X-Client-Version'] = $rootScope.version;
					}
				})
			);
			var preloadDone = false;

			$rootScope.$on('$stateChangeStart', function(event, next, toParams) {
				if (preloadDone) {
					if (!permitted(event, next)) {
						event.preventDefault();
						notPermitted(next);
					}
					return;
				}
				preloadDone = true;
				event.preventDefault();
				$q.all(preloads).then(function() {
					$state.go(next, toParams);
				});
			});

			$rootScope._ = (<any>window)._;

			function initializeUserInfo() {
				return currentUserInfo.initContext().then(function(data) {
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
				'resourceplanner.archiveschedule',
				'dataprotection'
			];

			function initializePermissionCheck() {
				return areasService.getAreasWithPermission().then(function(data) {
					permittedAreas = data;
					return areasService.getAreasList().then(function(data) {
						areas = data;
					});
				});
			}

			function permitted(event, next) {
				var name = next.name.split('.')[0];
				var url = next.url && next.url.split('/')[1];

				var permitted = alwaysPermittedAreas.some(function(a) {
					return a === next.name.toLowerCase();
				});

				if (!permitted)
					permittedAreas.forEach(function(area) {
						if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
							permitted = true;
						} else if (
							url &&
							(area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)
						) {
							permitted = true;
						}
					});

				return permitted;
			}

			function notPermitted(next) {
				$state.go('main');
				var moduleName;
				var name = next.name.split('.')[0];
				var url = next.url && next.url.split('/')[1];
				areas.forEach(function(area) {
					if (name && (area.InternalName.indexOf(name) > -1 || name.indexOf(area.InternalName) > -1)) {
						moduleName = area.Name;
					} else if (url && (area.InternalName.indexOf(url) > -1 || url.indexOf(area.InternalName) > -1)) {
						moduleName = area.Name;
					}
				});
				noticeService.error(
					"<span class='test-alert'></span>" +
						$translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', moduleName),
					null,
					false
				);
			}

			TabShortCut.unifyFocusStyle();
		}
	]);
