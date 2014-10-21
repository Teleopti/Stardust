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
/*
var wfm = angular.module('wfm', ["ui.router"]);
wfm.config([          '$stateProvider', '$urlRouterProvider',function($stateProvider, $urlRouterProvider){

    // For any unmatched url, send to /route1
    $urlRouterProvider.otherwise("/route1")

    $stateProvider
        .state('route1', {
            url: "/route1",
            templateUrl: "route1.html"
        })
        .state('route1.list', {
            url: "/list",
            templateUrl: "route1.list.html",
            controller: function($scope){
                $scope.items = ["A", "List", "Of", "Items"];
            }
        })

        .state('route2', {
            url: "/route2",
            templateUrl: "route2.html"
        })
        .state('route2.list', {
            url: "/list",
            templateUrl: "route2.list.html",
            controller: function($scope){
                $scope.things = ["A", "Set", "Of", "Things"];
            }
        })
}]);*/
