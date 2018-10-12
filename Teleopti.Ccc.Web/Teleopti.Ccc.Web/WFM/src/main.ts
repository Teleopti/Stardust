import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { downgradeComponent, downgradeModule } from '@angular/upgrade/static';
import { IControllerConstructor, IRootScopeService } from 'angular';
import { AddAppPageComponent, ListPageComponent } from './app/api-access/components';
import { AppModule } from './app/app.module';
import { ChangePasswordComponent } from './app/authentication/components/change-password/change-password.component';
import { BootstrapComponent } from './app/bootstrap/bootstrap.component';
import { SettingsMenuComponent } from './app/navigation/components';
import {
	AppLogonPageComponent,
	GrantPageComponent,
	IdentityLogonPageComponent,
	RevokePageComponent,
	SearchPageComponent,
	TitleBarComponent
} from './app/people/components';
import { FeedbackMessageComponent } from './app/shared/components';
import { environment } from './environments/environment';
import { MainController } from './main.controller';

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
	'wfm.apiaccess',
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
	'wfm.skillPickerOld',
	'wfm.treePicker',
	'wfm.card-panel',
	'wfm.skillGroup',
	'wfm.calendarPicker',
	'wfm.popup',
	'wfm.gamification',
	'wfm.btnGroup',
	'wfm.ai'
]);

wfm.controller('MainController', MainController as IControllerConstructor);

const downgradeHelper = (downgradeName: string, component) => {
	const downgradedComponent = downgradeComponent({ component }) as angular.IDirectiveFactory;
	wfm.directive(downgradeName, downgradedComponent);
};

downgradeHelper('ng2ApiAccessListPage', ListPageComponent);
downgradeHelper('ng2ApiAccessAddAppPage', AddAppPageComponent);
downgradeHelper('ng2PeopleTitleBar', TitleBarComponent);
downgradeHelper('ng2PeopleSearchPage', SearchPageComponent);
downgradeHelper('ng2PeopleGrantPage', GrantPageComponent);
downgradeHelper('ng2PeopleRevokePage', RevokePageComponent);
downgradeHelper('ng2PeopleAppLogonPage', AppLogonPageComponent);
downgradeHelper('ng2PeopleIdentityLogonPage', IdentityLogonPageComponent);
downgradeHelper('ng2ChangePassword', ChangePasswordComponent);
downgradeHelper('ng2FeedbackMessage', FeedbackMessageComponent);
downgradeHelper('ng2Bootstrap', BootstrapComponent);
downgradeHelper('ng2SettingsMenu', SettingsMenuComponent);

wfm.config([
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
			templateProvider: [
				'$templateRequest',
				function(templateRequest) {
					return templateRequest('html/main.html');
				}
			]
		});

		$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
		$translateProvider.useUrlLoader('../api/Global/Language');

		$translateProvider.preferredLanguage('en');
		$httpProvider.interceptors.push('httpInterceptor');
		$mdGestureProvider.skipClickHijack();

		tmhDynamicLocaleProvider.localeLocationPattern('dist/angular-i18n/angular-locale_{{locale}}.js');
		//	tmhDynamicLocaleProvider.defaultLocale("en-gb");  -- causes problems with unit tests due to reinit of scope
	}
]).run([
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
	'rtaDataService',
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
		rtaDataService,
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
		preloads.push(
			$q.all([initializeUserInfo(), initializePermissionCheck()]).then(function() {
				// any preloads than requires selected business unit and/or permission check
				if (permitted('rta', undefined)) rtaDataService.load(); // dont return promise, async call
			})
		);
		preloads.push(
			$http.get('../api/Global/Version').then(function(response) {
				$rootScope.version = response.data;
				$http.defaults.headers.common['X-Client-Version'] = $rootScope.version;
			})
		);
		var preloadDone = false;

		$rootScope.$on('$stateChangeStart', function(event, next, toParams) {
			if (preloadDone) {
				if (!permitted(internalNameOf(next), urlOf(next))) {
					event.preventDefault();
					notPermitted(internalNameOf(next));
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
			'rtatool',
			'rtatracer',
			'resourceplanner',
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

		function permitted(name, url) {
			var permitted = alwaysPermittedAreas.some(function(a) {
				return a === name.toLowerCase();
			});
			if (!permitted)
				permitted = permittedAreas.some(function(a) {
					if (url && (a.InternalName.indexOf(url) > -1 || url.indexOf(a.InternalName) > -1)) return true;
					else return a.InternalName === name;
				});
			return permitted;
		}

		function notPermitted(internalName) {
			noticeService.error(
				"<span class='test-alert'></span>" +
					$translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', nameOf(internalName)),
				null,
				false
			);
			$state.go('main');
		}

		function internalNameOf(o) {
			var name = o.name;
			name = name.split('.')[0];
			name = name.split('-')[0];
			return name;
		}

		function urlOf(o) {
			return o.url && o.url.split('/')[1];
		}

		function nameOf(internalName) {
			var name;
			areas.forEach(function(area) {
				if (area.InternalName == internalName) name = area.Name;
			});
			return name;
		}

		TabShortCut.unifyFocusStyle();
	}
]);
