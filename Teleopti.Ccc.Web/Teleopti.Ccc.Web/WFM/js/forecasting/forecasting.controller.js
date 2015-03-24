'use strict';

angular.module('wfm.forecasting', [])
	.controller('ForecastingCtrl', [
		'$scope', '$state',
		function ($scope, $state) {
			var startDate = moment().add(1, 'months').startOf('month').toDate();
			var endDate = moment().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			$scope.nextStep = function (period) {
				$state.go('forecasting.target', { period: period }); //there's probably a better way to do that
			};
		}
	]
	)
	
	.controller('ForecastingRunCtrl', ['$scope', '$stateParams', '$http',
		function ($scope, $stateParams, $http) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: $scope.targets })).
				success(function (data, status, headers, config) {
					$scope.result = { success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.' };
				}).
				error(function (data, status, headers, config) {
					$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
				});
		}]
);