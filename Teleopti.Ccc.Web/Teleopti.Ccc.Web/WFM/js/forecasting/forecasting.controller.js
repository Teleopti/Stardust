'use strict';

angular.module('wfm.forecasting', [])
.controller('ForecastingCtrl', ['$scope', '$state', 'Forecasting',
        function ($scope, $state, Forecasting) {
        	$scope.skillsDisplayed = [];

        	Forecasting.skills.query().$promise.then(function (result) {
        		$scope.skillsDisplayed = result;
        	});

        	var startDate = moment().add(1, 'months').startOf('month').toDate();
        	var endDate = moment().add(2, 'months').startOf('month').toDate();
        	$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

        	$scope.startPeriod = function (period) {
        		$state.go('forecasting.run', { period: period }); //there's probably a better way to do that
        	};
        }]
)
.controller('ForecastingRunCtrl', ['$scope', '$stateParams', '$http',
        function ($scope, $stateParams, $http) {

        	$scope.period = $stateParams.period;
        	$http.post('../api/Forecasting/forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate })).
                success(function (data, status, headers, config) {
                	$scope.result = { success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.', accuracy: data[0].Accuracy == 'NaN' ? 'Not enough historical data for measuring.' : data[0].Accuracy + '%' };
		        }).
                error(function (data, status, headers, config) {
                	$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
                });
        }]
);