import { enableProdMode, StaticProvider } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { downgradeComponent as ngDowngradeComponent, downgradeModule } from '@angular/upgrade/static';
import { IRootScopeService } from 'angular';
import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';
import { apiAccessComponents, apiAccessRouterConfig } from './app/api-access/api-access.module';
import { appComponents, AppModule } from './app/app.module';
import { authenticationComponents } from './app/authentication/authentication.module';
import { menuComponents } from './app/menu/menu.module';
import { navigationComponents } from './app/navigation/navigation.module';
import { peopleComponents, peopleRouterConfig } from './app/people/people.module';
import { insightsComponents, insightsRouterConfig } from './app/insights/insights.module';
import { sharedComponents } from './app/shared/shared.module';
import { environment } from './environments/environment';
import { MainController } from './main.controller';
import { mainInitializer } from './main.initializer';
import { DowngradeableComponent, RouterConfigFunction } from './types';
export interface IWfmRootScopeService extends IRootScopeService {
	_: any;
	isAuthenticated: boolean;
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
	'wfm.versionService',
	'wfm.http',
	'wfm.exceptionHandler',
	'wfm.permissions',
	'wfm.peopleold',
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
	'wfm.teamSchedule',
	'wfm.intraday',
	'wfm.requests',
	'wfm.reports',
	'wfm.signalR',
	'wfm.datePicker',
	'wfm.dateRangePicker',
	'wfm.utilities',
	'wfm.staffing',
	'wfm.templates',
	'wfm.badge',
	'wfm.skillPicker',
	'wfm.skillPickerOld',
	'wfm.treePicker',
	'wfm.card-panel',
	'wfm.skillGroup',
	'wfm.popup',
	'wfm.gamification',
	'wfm.btnGroup'
]);

wfm.controller('MainController', MainController);

/**
 * Downgrade components with a graceful syntax
 * @param downgradableComponents a list of components
 * which implements the DowngradableComponents interface
 */
const downgradeHelper = (downgradableComponents: DowngradeableComponent[] | DowngradeableComponent) => {
	if (Array.isArray(downgradableComponents)) {
		downgradableComponents.forEach(downgradeHelper);
	} else {
		const { ng1Name, ng2Component } = downgradableComponents;
		const downgradedComponent = ngDowngradeComponent({ component: ng2Component }) as angular.IDirectiveFactory;
		wfm.directive(ng1Name, downgradedComponent);
	}
};

// Use this to downgrade module components
downgradeHelper(peopleComponents);
downgradeHelper(sharedComponents);
downgradeHelper(navigationComponents);
downgradeHelper(authenticationComponents);
downgradeHelper(apiAccessComponents);
downgradeHelper(appComponents);
downgradeHelper(menuComponents);
downgradeHelper(insightsComponents);

/**
 * Use this if your module is purely Angular and you want mount some routes
 */
const routerHelper = (routerConfig: RouterConfigFunction) => {
	wfm.config(['$stateProvider', '$urlRouterProvider', routerConfig]);
};

routerHelper(peopleRouterConfig);
routerHelper(insightsRouterConfig);
routerHelper(apiAccessRouterConfig);

wfm.config([
	'$stateProvider',
	'$urlRouterProvider',
	'$translateProvider',
	'$httpProvider',
	'$mdGestureProvider',
	'tmhDynamicLocaleProvider',
	function(
		$stateProvider: IStateProvider,
		$urlRouterProvider: IUrlRouterProvider,
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
		// tmhDynamicLocaleProvider.defaultLocale("en-gb");  -- causes problems with unit tests due to reinit of scope
	}
]);

wfm.run(mainInitializer);
