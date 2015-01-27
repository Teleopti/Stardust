'use strict';

var wfm = angular.module('wfm', [
    'ui.router',
    'ui.bootstrap',
	'ui.tree',
    'ngMaterial',
    'angularMoment',
    'wfmCtrls'
]);
wfm.config(['$stateProvider', '$urlRouterProvider',function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise("forecasting");
    $stateProvider.state('main', {
        url:'/',
        templateUrl: 'html/main.html',
        controller: 'MainCtrl'
    }).state('forecasting', {
        url:'/forecasting',
        templateUrl: 'html/forecasting.html',
        controller: 'ForecastingCtrl'
    }).state('forecasting.run', {
        params:{period: {}},
        templateUrl: 'html/forecasting-run.html',
        controller: 'ForecastingRunCtrl'
    }).state('permissions', {
    	url: '/permissions',
    	templateUrl: 'html/permissions.html',
    	controller: 'PermissionsCtrl'
    });
}]).run(['$rootScope', '$http', '$state', function ($rootScope, $http, $state) {
    var timeout = Date.now() + 10000;
    $rootScope.$on('$stateChangeStart', function (event, next, toParams) {
        if(Date.now() > timeout ) { // TODO : extract it in a service
            event.preventDefault();
            var context = $http.get('../../api/Forecasting/forecast');
            context.error(function () {
                window.location = '../../';
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