'use strict';

var wfm = angular.module('wfm', [
    'ui.router',
    'wfmCtrls'
]);
wfm.config(['$stateProvider', '$urlRouterProvider',function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise("/");
    //debugger;
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
    });
}]).run(['$rootScope', '$http', '$q', '$location'], function ($rootScope, $http, $q, $location) {
	var context = $http.get('../../Anywhere/Application/NavigationContent');
	$rootScope.$on('$routeChangeStart', function (event, next, current) {
		$q.all(context);
		context.error(function() { $location.path('../../'); });
		context.success(function(data) {});
	});
});