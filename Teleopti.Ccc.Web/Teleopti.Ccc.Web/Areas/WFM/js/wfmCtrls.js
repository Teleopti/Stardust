'use strict';

var wfmCtrls = angular.module('wfmCtrls', []);

wfmCtrls.controller('MainCtrl',
    function ($scope) {

    }
);

wfmCtrls.controller('ForecastingCtrl', ['$scope', '$state',
    function ($scope, $state) {
       $scope.startPeriod = function (period){
           $state.go('forecasting.run', {period: period}); //there's probably a better way to do that
        }
    }]
);

wfmCtrls.controller('ForecastingRunCtrl',[ '$scope','$stateParams',
    function ($scope, $stateParams) {
        $scope.period = $stateParams.period;
       $scope.states =['done'];

    }]
);