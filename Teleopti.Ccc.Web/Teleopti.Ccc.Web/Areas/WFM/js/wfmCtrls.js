'use strict';

var wfmCtrls = angular.module('wfmCtrls', []);

wfmCtrls.controller('MainCtrl',
    function ($scope) {

    }
);

wfmCtrls.controller('ForecastingCtrl', ['$scope', '$state',
        function ($scope, $state) {
            var startDate = moment().add(1,'months').startOf('month').toDate();
            var endDate = moment().add(2, 'months').startOf('month').toDate();
            $scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

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
                    $scope.result = {success: true, message: 'You now have an updated forecast in your default scenario.'};
                }).
                error(function(data, status, headers, config) {
                    $scope.result = {success: false, message: 'The forecast has failed. Please try again later'};
                });
        }]
);