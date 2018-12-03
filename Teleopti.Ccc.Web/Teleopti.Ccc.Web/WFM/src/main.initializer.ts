import { IState, IStateService } from 'angular-ui-router';
import { Area } from './app/menu/shared/area.service';
import { IWfmRootScopeService } from './main';

export interface Areas {
	permitted: Area[];
	available: Area[];
	alwaysPermitted: string[];
}

export const mainInitializer = [
	'$rootScope',
	'$state',
	'$translate',
	'$locale',
	'CurrentUserInfo',
	'SupportEmailService',
	'Toggle',
	'areasService',
	'NoticeService',
	'rtaDataService',
	'$window',
	'$http',
	function(
		$rootScope: IWfmRootScopeService,
		$state,
		$translate,
		$locale,
		currentUserInfo,
		supportEmailService,
		toggleService,
		areasService,
		noticeService,
		rtaDataService,
		$window
	) {
		$rootScope._ = (<any>window)._;
		$rootScope.isAuthenticated = false;

		const areas: Areas = {
			permitted: [],
			available: [],
			alwaysPermitted: [
				'main',
				'skillprio',
				'teapot',
				'rtatool',
				'rtatracer',
				'resourceplanner',
				'dataprotection'
			]
		};

		let preloadDone = false;
		const preload = Promise.all([
			areasService.getAreasList(),
			areasService.getAreasWithPermission(),
			toggleService.togglesLoaded,
			supportEmailService.init(),
			currentUserInfo.initContext().then(userPreferences => $translate.use(userPreferences.Language))
		]).then(([areasAvailable, permittedAreas]) => {
			$rootScope.isAuthenticated = true;

			areas.available = areasAvailable;
			areas.permitted = permittedAreas;

			if (isPermittedArea(areas, 'rta')) rtaDataService.load();
		});

		$rootScope.$on('$localeChangeSuccess', () => {
			if ($locale.id === 'zh-cn') $locale.DATETIME_FORMATS.FIRSTDAYOFWEEK = 0;
		});

		$rootScope.$on('$stateChangeStart', (event, next: IState, toParams) => {
			if (!preloadDone) {
				preloadDone = true; // Why is this done!?
				event.preventDefault();
				preload.then(() => $state.go(next, toParams));
			} else if (!isPermittedArea(areas, internalNameOf(next), urlOfState(next))) {
				event.preventDefault();
				handleNotPermitted(areas, noticeService, $translate, $state, next);
			}
		});

		$rootScope.$on('$stateChangeSuccess', () => {
			if ($window.appInsights) $window.appInsights.trackPageView($state.current.name);
		});
	}
];

export function handleNotPermitted(areas, noticeService, $translate, $state: IStateService, state: IState) {
	const stateInternalName = internalNameOf(state);
	const stateName = nameOfArea(areas, stateInternalName);

	noticeService.error(
		`<span class="test-alert"></span>` +
			$translate.instant('NoPermissionToViewWFMModuleErrorMessage').replace('{0}', stateName),
		null,
		false
	);
	$state.go('main');
}

export function internalNameOf(state: IState): string {
	return state.name.split(/[\.-]/)[0];
}

export function isPermittedArea(areas: Areas, name: string, url?: string): boolean {
	const isAlwaysPermitted = areas.alwaysPermitted.includes(name.toLowerCase());
	const isOtherwisePermitted = areas.permitted.some(area => {
		if (url && (area.InternalName.includes(url) || url.includes(area.InternalName))) return true;
		return area.InternalName === name;
	});
	return isAlwaysPermitted || isOtherwisePermitted;
}

export function nameOfArea(areas: Areas, internalName: string): string {
	const area = areas.available.find(a => a.InternalName === internalName);
	return area ? area.Name : undefined;
}

export function urlOfState(state: IState): string {
	return state.url && state.url.toString().split('/')[1];
}
