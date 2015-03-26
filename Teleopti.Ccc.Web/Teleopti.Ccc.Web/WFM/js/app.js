'use strict';

var wfm = angular.module('wfm', [
	'ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'restService',
	'restSearchService',
	'restNotificationService',
	'forecastingService',
	'outboundService',
	'wfmCtrls',
	'wfm.permissions',
	'wfm.people',
	'wfm.outbound',
	'wfm.forecasting',
	'wfm.forecasting.target',
	'wfm.searching',
	'wfm.notifications',
	'wfm.notice'
]);
wfm.config([
	'$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {
		$urlRouterProvider.otherwise("forecasting");
		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html',
			controller: 'MainCtrl'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'html/forecasting/forecasting.html',
			controller: 'ForecastingCtrl'
    }).state('forecasting.target', {
    	params: { period: {} },
    	templateUrl: 'html/forecasting/forecasting-target.html',
    	controller: 'ForecastingTargetCtrl'
		}).state('forecasting.run', {
        params:{ period: {}, targets: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunCtrl'
		}).state('forecasting.runall', {
			params: { period: {} },
			templateUrl: 'html/forecasting/forecasting-run.html',
			controller: 'ForecastingRunAllCtrl'
		}).state('permissions', {
			url: '/permissions',
			templateUrl: 'html/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		}).state('outbound', {
			url: '/outbound',
			templateUrl: 'html/outbound/campaign-list.html',
			controller: 'OutboundListCtrl'
		}).state('outbound.edit', {
			url: '/campaign/:id',
			templateUrl: 'html/outbound/campaign-edit.html',
			controller: 'OutboundEditCtrl'
		}).state('outbound.schedule', {
			url: '/campaign/:id/schedule',
			templateUrl: 'html/outbound/campaign-schedule.html',
			controller: 'OutboundEditCtrl'
		}).state('people', {
			url: '/people',
			templateUrl: 'html/people/people.html',
			controller: 'PeopleCtrl'
		});
	}
]).run([
	'$rootScope', '$http', '$state', function($rootScope, $http, $state) {
		var timeout = Date.now() + 10000;

		var checkCurrentUser = function() {
			return $http.get('../api/Global/User/CurrentUser');
		};

		var userNotAuthenticatedHandler = function() {
			if (window.location.hash) {
				var d = new Date();
				d.setTime(d.getTime() + (5 * 60 * 1000));
				var expires = 'expires=' + d.toUTCString();
				document.cookie = 'returnHash' + '=' + window.location.hash + '; ' + expires + '; path=/';
			}
			window.location = 'Authentication';
		};

		var increaseTimeout = function() {
			timeout = Date.now() + 10000;
		};

		$rootScope.$on('$stateChangeStart', function(event, next, toParams) {

			if (Date.now() > timeout) { // TODO : extract it in a service
				event.preventDefault();
				var context = checkCurrentUser();
				context.error(userNotAuthenticatedHandler);
				context.success(function(data) {
					increaseTimeout();
					$state.go(next, toParams);
				});
			}
		});

		var startContext = checkCurrentUser();
		startContext.error(userNotAuthenticatedHandler);
		startContext.success(function (data) {
			increaseTimeout();

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
			});
		});
	}
]);