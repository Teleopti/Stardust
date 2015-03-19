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
	'wfmCtrls',
	'wfm.permissions',
	'wfm.forecasting',
	'wfm.searching',
	'wfm.notifications'
]);
wfm.config(['$stateProvider', '$urlRouterProvider',function ($stateProvider, $urlRouterProvider) {
	$urlRouterProvider.otherwise("forecasting");
    $stateProvider.state('main', { 
        url:'/',
        templateUrl: 'html/main.html',
        controller: 'MainCtrl'
    }).state('forecasting', {
        url:'/forecasting',
        templateUrl: 'html/forecasting/forecasting.html',
        controller: 'ForecastingCtrl'
    }).state('forecasting.run', {
        params:{period: {}},
        templateUrl: 'html/forecasting/forecasting-run.html',
        controller: 'ForecastingRunCtrl'
    }).state('permissions', {
    	url: '/permissions',
    	templateUrl: 'html/permissions/permissions.html',
    	controller: 'PermissionsCtrl'
    });
}]).run(['$rootScope', '$http', '$state', function ($rootScope, $http, $state) {
    var timeout = Date.now() + 10000;
    $rootScope.$on('$stateChangeStart', function (event, next, toParams) {

        if(Date.now() > timeout ) { // TODO : extract it in a service
            event.preventDefault();
            var context = $http.get('../api/Global/User/CurrentUser');
            context.error(function () {
            	if (window.location.hash) {
            		var d = new Date();
            		d.setTime(d.getTime() + (5 * 60 * 1000));
            		var expires = 'expires=' + d.toUTCString();
            		document.cookie = 'returnHash' + '=' + window.location.hash + '; ' + expires + '; path=/';
            	}
            	window.location = 'Authentication';
            });
            context.success(function (data) {
                timeout = Date.now() + 10000;
                $state.go(next, toParams);
            });
        }
    });
    var ab1 = new ABmetrics();
    ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
    $rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
        ab1.sendPageView();
    });
    
}]);