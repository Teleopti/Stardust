'use strict';

var wfmCtrls = angular.module('wfmCtrls', []);

wfmCtrls.controller('MainCtrl',
    function ($scope) {

    }
);

wfmCtrls.controller('ForecastingCtrl', ['$scope', '$state',
        function ($scope, $state) {
            var today = new Date();
            $scope.period = { startDate: today.toLocaleDateString(), endDate: today.toLocaleDateString() }; //use moment to get first day of next month

            $scope.startPeriod = function (period) {
                $state.go('forecasting.run', {period: period}); //there's probably a better way to do that
            }
        }]
);

wfmCtrls.controller('ForecastingRunCtrl', [ '$scope', '$stateParams','$http',
        function ($scope, $stateParams, $http) {

            $scope.period = $stateParams.period;
            //api/Forecasting/forecast/QuickForecast
            $http.post('../../api/Forecasting/forecast', JSON.stringify({ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate})).
                success(function(data, status, headers, config) {
                    $scope.result = 'DONE (success)';
                }).
                error(function(data, status, headers, config) {
                    $scope.result = 'DONE (failure)';
                });
        }]
);