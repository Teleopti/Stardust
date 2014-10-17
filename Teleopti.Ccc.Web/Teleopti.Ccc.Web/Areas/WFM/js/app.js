'use strict';

angular.module('wfm', [
    'ngRoute',
    'wfmCtrls'
]).config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/', {
        templateUrl: 'html/main.html',
        controller: 'MainCtrl'
    }).otherwise({redirectTo: '/'});
}]);