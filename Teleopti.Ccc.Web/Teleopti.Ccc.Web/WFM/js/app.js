'use strict';

var externalModules = angular.module('externalModules', ['ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'pascalprecht.translate',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection'
]);

var wfm = angular.module('wfm', [
	'externalModules',
	'forecastingService',	
	'peopleSearchService',
	'outboundServiceModule',
	'restRtaService',
	'wfmCtrls',
	'wfm.permissions',
	'wfm.people',
	'wfm.outbound',
	'wfm.forecasting',
	'wfm.forecasting.target',
	'wfm.forecasting.dev',
	'wfm.resourceplanner',
	'wfm.searching',
	'wfm.seatMap',
	'wfm.seatPlan',
	'wfm.notifications',
	'wfm.notice',
	'wfm.areas',
	'wfm.help',
	'wfm.rta'
]);
wfm.config([
	'$stateProvider', '$urlRouterProvider', '$translateProvider', function ($stateProvider, $urlRouterProvider, $translateProvider) {



		$urlRouterProvider.otherwise("forecasting");
		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html',
			controller: 'MainCtrl'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'html/forecasting/forecasting.html',
			controller: 'ForecastingCtrl'
		}).state('forecasting-target', {
			params: { period: {} },
			templateUrl: 'html/forecasting/forecasting-target.html',
			controller: 'ForecastingTargetCtrl'
		}).state('forecasting-run', {
			params: { period: {}, targets: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunCtrl'
		}).state('forecasting-runall', {
			params: { period: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunAllCtrl'
		}).state('forecasting-dev', {
			params: { workloadId: {} },
			templateUrl: 'html/forecasting/forecasting-dev.html',
			controller: 'ForecastingDevCtrl'
		}).state('resourceplanner', {
				url: '/resourceplanner',
				templateUrl: 'js/resourceplanner/resourceplanner.html',
				controller: 'ResourcePlannerCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'js/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: { result: {} },
			templateUrl: 'js/resourceplanner/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('permissions', {
			params: { id: {} },
			url: '/permissions',
			templateUrl: 'html/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		}).state('outbound', {
			url: '/outbound',
			templateUrl: 'html/outbound/campaign-prototype.html',
			controller: 'OutboundListCtrl'
		}).state('outbound-create', {
			url: '/outbound/create',
			templateUrl: 'html/outbound/campaign-create.html',
			controller: 'OutboundCreateCtrl'
		}).state('outbound-prototype', {
			url: '/outbound/prototype',
			templateUrl: 'html/outbound/campaign-prototype.html',
			controller: 'OutboundListCtrl'
		}).state('outbound.edit', {
			url: '/campaign/:Id',
			templateUrl: 'html/outbound/campaign-edit.html',
			controller: 'OutboundEditCtrl'
		}).state('outbound.forecasting', {
			url: '/campaign/:id/forecasting',
			templateUrl: 'html/outbound/campaign-forecasting.html',
			controller: 'OutboundEditCtrl'
		}).state('people', {
			url: '/people',
			templateUrl: 'html/people/people.html',
			controller: 'PeopleCtrl'
		}).state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html'
		}).state('rta', {
			url: '/rta',
			templateUrl: 'js/rta/html/rta.html',
			controller: 'RtaCtrl'
		});

		$translateProvider.useUrlLoader('../api/Global/Language');
		$translateProvider.preferredLanguage('en');
	}
]).run([
	'$rootScope', '$http', '$state', '$translate', 'i18nService', 'amMoment', 'HelpService', function ($rootScope, $http, $state, $translate, i18nService, angularMoment, HelpService) {
		var timeout = Date.now() + 10000;
		$rootScope.isAuthenticated = false;

		var checkCurrentUser = function () {
			return $http.get('../api/Global/User/CurrentUser');
		};

		var userNotAuthenticatedHandler = function () {
			if (window.location.hash) {
				var d = new Date();
				d.setTime(d.getTime() + (5 * 60 * 1000));
				var expires = 'expires=' + d.toUTCString();
				document.cookie = 'returnHash' + '=' + window.location.hash + '; ' + expires + '; path=/';
			}
			window.location = 'Authentication';
		};

		var increaseTimeout = function () {
			timeout = Date.now() + 10000;
		};

		$rootScope.$on('$stateChangeStart', function (event, next, toParams) {

			if (Date.now() > timeout) { // TODO : extract it in a service
				event.preventDefault();
				var context = checkCurrentUser();
				context.error(userNotAuthenticatedHandler);
				context.success(function (data) {
					increaseTimeout();
					$state.go(next, toParams);
				});
			}
		});

		var startContext = checkCurrentUser();
		startContext.error(userNotAuthenticatedHandler);
		startContext.success(function (data) {
			$rootScope.isAuthenticated = true;
			$translate.fallbackLanguage('en');
			$translate.use(data.Language);
			angularMoment.changeLocale(data.DateFormat);
			increaseTimeout();

			// i18nService is for UI Grid localization.
			// Languages supported by it is less than languages in server side (Refer to http://ui-grid.info/docs/#/tutorial/104_i18n).
			// Need do some primary language checking.
			var currentLang = "en";
			var serverSideLang = data.Language.toLowerCase();
			var dashIndex = serverSideLang.indexOf("-");
			var primaryLang = dashIndex > -1 ? serverSideLang.substring(0, dashIndex) : serverSideLang;
			var langs = i18nService.getAllLangs();
			if (langs.indexOf(serverSideLang) > -1) {
				currentLang = serverSideLang;
			} else if (langs.indexOf(primaryLang) > -1) {
				currentLang = primaryLang;
			}
			i18nService.setCurrentLang(currentLang);

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
				HelpService.updateState($state.current.name);
			});
		});
	}
]);