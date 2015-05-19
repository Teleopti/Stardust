'use strict';

var wfm = angular.module('wfm', [
	'ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'pascalprecht.translate',
	'restService',
	'restSearchService',
	'restNotificationService',
	'restResourcePlannerService',
	'forecastingService',
	'outboundService',
	'restAreasService',
	'peopleSearchService',
	'restRtaService',
	'wfmCtrls',
	'wfm.permissions',
	'wfm.people',
	'wfm.outbound',
	'wfm.forecasting',
	'wfm.forecasting.target',
	'wfm.resourceplanner',
	'wfm.resourceplanner.report',
	'wfm.searching',
	'wfm.seatMap',
	'wfm.seatPlan',
	'wfm.notifications',
	'wfm.notice',
	'wfm.areas',
	'wfm.help',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection',
	'wfm.rta',
	'ncy-angular-breadcrumb',
	'permissionsFilters'
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
		}).state('resourceplanner', {
			url: '/resourceplanner',
			templateUrl: 'js/resourceplanner/resourceplanner.html',
			controller: 'ResourceplannerCtrl'
		}).state('resourceplannerreport', {
			params: { result: {} },
			templateUrl: 'js/resourceplanner/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('permissions', {
			url: '/permissions',
			templateUrl: 'html/permissions/permissions.html',
			controller: 'PermissionsCtrl',

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
			controller: 'PeopleCtrl'
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
	'$rootScope', '$http', '$state', '$translate', 'amMoment', 'HelpService', function ($rootScope, $http, $state, $translate, angularMoment, HelpService) {
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

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
				HelpService.updateState($state.current.name);
			});
		});
	}
]);