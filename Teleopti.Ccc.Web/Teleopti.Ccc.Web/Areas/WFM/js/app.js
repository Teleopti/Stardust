'use strict';

var wfm = angular.module('wfm', [
    'ui.router',
    'wfmCtrls'
]);
wfm.config([          '$stateProvider', '$urlRouterProvider',function ($stateProvider, $urlRouterProvider) {
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
}]);