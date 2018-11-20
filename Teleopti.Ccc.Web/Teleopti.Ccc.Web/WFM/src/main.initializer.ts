import { IQService, ITimeoutService } from 'angular';
import { IState } from 'angular-ui-router';
import { IWfmRootScopeService } from './main';

export const mainInitializer = [
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
	'$window',
	'$http',
	function(
		$rootScope: IWfmRootScopeService,
		$state,
		$translate,
		$timeout: ITimeoutService,
		$locale,
		currentUserInfo,
		toggleService,
		areasService,
		noticeService,
		TabShortCut,
		rtaDataService,
		$q: IQService,
		$window
	) {
		$rootScope.isAuthenticated = false;

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
		var preloadDone = false;

		$rootScope.$on('$stateChangeStart', function(event, next: IState, toParams) {
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

		// application insight
		$rootScope.$on('$stateChangeSuccess', function() {
			if ($window.appInsights) $window.appInsights.trackPageView($state.current.name);
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
		var alwaysPermittedAreas: string[] = [
			'main',
			'skillprio',
			'teapot',
			'rtatool',
			'rtatracer',
			'resourceplanner',
			'dataprotection'
		];

		function initializePermissionCheck(): Promise<void> {
			return areasService.getAreasWithPermission().then(function(data) {
				permittedAreas = data;
				return areasService.getAreasList().then(function(data) {
					areas = data;
				});
			});
		}

		function permitted(name: string, url: string): boolean {
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

		function notPermitted(internalName: string) {
			noticeService.error(
				"<span class='test-alert'></span>" +
					$translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', nameOf(internalName)),
				null,
				false
			);
			$state.go('main');
		}

		function internalNameOf(o: IState): string {
			var name = o.name;
			name = name.split('.')[0];
			name = name.split('-')[0];
			return name;
		}

		function urlOf(o: IState) {
			return o.url && o.url.toString().split('/')[1];
		}

		function nameOf(internalName: string): string {
			var name;
			areas.forEach(function(area) {
				if (area.InternalName == internalName) name = area.Name;
			});
			return name;
		}

		TabShortCut.unifyFocusStyle();
	}
];
