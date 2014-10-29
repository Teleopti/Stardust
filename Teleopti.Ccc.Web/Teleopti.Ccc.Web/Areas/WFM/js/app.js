'use strict';

var wfm = angular.module('wfm', [
    'ui.router',
    'wfmCtrls'
]);
wfm.config(['$stateProvider', '$urlRouterProvider',function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise("/");
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
}]).run(['$rootScope', '$http', '$state', function ($rootScope, $http, $state) {
    var timeout = Date.now() + 10000;
	$rootScope.$on('$stateChangeStart', function (event, next, toParams) {
        if(Date.now() > timeout ) {
            event.preventDefault();
            var context = $http.get('../../Anywhere/Application/NavigationContent');
            context.error(function () {
                window.location = '../../';
            });
            context.success(function (data) {
                timeout = Date.now() + 10000;
                $state.go(next, toParams);
            });
        }
	});
}]);