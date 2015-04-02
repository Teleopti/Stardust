'use strict';

angular.module('wfm.forecasting', [])
	.controller('ForecastingCtrl', [
		'$scope', '$state',
		function ($scope, $state) {
			var startDate = moment().add(1, 'months').startOf('month').toDate();
			var endDate = moment().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			$scope.expressNextStep = function (period) {
				$state.go('forecasting.runall', { period: period });
			};

			$scope.advancedNextStep = function (period) {
				$state.go('forecasting.target', { period: period });
			};
		}
	]
	)
	.controller('ForecastingRunCtrl', ['$scope', '$stateParams', '$http', 'Forecasting',
		function ($scope, $stateParams, $http, Forecasting) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			var workloadIds = [];
			angular.forEach($stateParams.targets, function (workload) {
				workloadIds.push(workload.Id);
			});
			$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: workloadIds })).
				success(function (data, status, headers, config) {
					$scope.result = { success: true, message: 'You now have an updated forecast for the following workloads in your default scenario, based on last year\'s data:' };
				}).
				error(function (data, status, headers, config) {
					$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
				});
		}])
	.controller('ForecastingRunAllCtrl', ['$scope', '$stateParams', '$http',
		function ($scope, $stateParams, $http) {

			$scope.period = $stateParams.period;

			$http.post('../api/Forecasting/ForecastAll', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate })).
				success(function (data, status, headers, config) {
					$scope.result = { success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.' };
				}).
				error(function (data, status, headers, config) {
					$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
				});
			
		}])
;