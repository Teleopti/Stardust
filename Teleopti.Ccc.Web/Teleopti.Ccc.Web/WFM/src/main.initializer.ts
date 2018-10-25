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
];
