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
	'ui.grid.selection',
	'ncy-angular-breadcrumb']);

var wfm = angular.module('wfm', [
	'externalModules',
	'forecastingService',
	'outboundService',
	'peopleSearchService',
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
	'$stateProvider', '$urlRouterProvider', '$translateProvider', '$breadcrumbProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $breadcrumbProvider) {
		$breadcrumbProvider.setOptions({
			template: '<ol class="breadcrumb">' +
            '<li ng-repeat="step in steps" ng-class="{active: $last}" ng-switch="$last || !!step.abstract">' +
            '<a ng-switch-when="false" ui-sref="{{step.name}}" ui-sref-opts="{ reload: true}">{{step.ncyBreadcrumbLabel}}</a>' +
            '<span ng-switch-when="true">{{step.ncyBreadcrumbLabel}}</span>' +
            '</li>' +
            '</ol>'
		});


		$urlRouterProvider.otherwise("forecasting");
		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html',
			controller: 'MainCtrl'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'html/forecasting/forecasting.html',
			controller: 'ForecastingCtrl',
			ncyBreadcrumb: {
				label: "{{'Forecasts' | translate}}"
			},
		}).state('forecasting.target', {
			params: { period: {} },
			templateUrl: 'html/forecasting/forecasting-target.html',
			controller: 'ForecastingTargetCtrl',
			ncyBreadcrumb: {
				label: "{{'Advanced' | translate}}"
			}
		}).state('forecasting.run', {
			params: { period: {}, targets: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunCtrl',
			ncyBreadcrumb: {
				label: 'forecasting results'
			}
		}).state('forecasting.runall', {
			params: { period: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunAllCtrl',
			ncyBreadcrumb: {
				label: 'forecasting results'
			}
		}).state('forecasting.dev', {
			params: { workloadId: {} },
			templateUrl: 'html/forecasting/forecasting-dev.html',
			controller: 'ForecastingDevCtrl',
			ncyBreadcrumb: {
				label: 'Forecast Dev view'
			}
		}).state('resourceplanner', {
				url: '/resourceplanner',
				templateUrl: 'js/resourceplanner/resourceplanner.html',
				controller: 'ResourcePlannerCtrl',
				ncyBreadcrumb: {
					label: 'resourceplanner'
				}
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'js/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplannerreport', {
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
			templateUrl: 'html/outbound/campaign-list.html',
			controller: 'OutboundListCtrl',
			ncyBreadcrumb: {
				label: "{{'Outbound' | translate}}"
			}
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
			controller: 'PeopleCtrl',
			ncyBreadcrumb: {
			label: "{{'People' | translate}}"
		}
		}).state('seatPlan', {
			url: '/seatPlan',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan',
			ncyBreadcrumb: {
				label: "{{'SeatPlan' | translate}}"
			}
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html',
			ncyBreadcrumb: {
				label: "{{'SeatMap' | translate}}"
			}
		}).state('rta', {
			url: '/rta',
			templateUrl: 'js/rta/html/rta.html',
			controller: 'RtaCtrl',
			ncyBreadcrumb: {
				label: "{{'RealTimeAdherenceOverview' | translate}}"
			}
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
			i18nService.setCurrentLang(data.Language);
			angularMoment.changeLocale(data.DateFormat);
			increaseTimeout();

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
				HelpService.updateState($state.current.name);
			});
		});
	}
]);