'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingAdvancedCtrl', ['$scope', '$state', '$stateParams', '$http', 'Toggle',
		function ($scope, $state, $stateParams, $http, toggleService) {
			$scope.workloadId = $stateParams.workloadId;
			$scope.chartInfo = {
				evaluationChartDataColumns: [
					{ id: "vh", type: "line", name: "Queue Statistics" },
					{ id: "vb", type: "line", name: "Forecast Method" }
				],
				queueStatisticsChartDataColumns: [
					{ id: "vh", type: "line", name: "Queue Statistics" },
					{ id: "vh2", type: "line", name: "Queue Statistics without Outliers" }
				],
				dataX: { id: "date" }
			};

			$scope.back = function () {
				$state.go("forecasting");
			};

			$scope.isQueueStatisticsEnabled = false;
			toggleService.isFeatureEnabled.query({ toggle: 'WfmForecast_QueueStatistics_32572' }).$promise.then(function (result) {
				$scope.isQueueStatisticsEnabled = result.IsEnabled;
			});

			var getQueueStatistics = function (workloadId, methodId) {
				$scope.queueStatisticsLoading = true;
				$http.post("../api/Forecasting/QueueStatistics", JSON.stringify({ WorkloadId: workloadId, MethodId: methodId })).
					success(function (data, status, headers, config) {
						$scope.queueStatisticsLoading = false;
						angular.forEach(data.QueueStatisticsDays, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						$scope.queueStatisticsChartData = data.QueueStatisticsDays;
					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to get queue statisctics." };
						$scope.queueStatisticsLoading = false;
					});
			};

			$scope.init = function () {
				//$scope.workloadName = workload.Name;
				$scope.noHistoricalDataForEvaluation = false;
				$scope.noHistoricalDataForForecasting = false;
				$scope.evaluationLoading = true;
				$scope.queueStatisticsLoading = true;
				$scope.evaluationChartData = [];
				$scope.queueStatisticsChartData = [];

				$http.post("../api/Forecasting/Evaluate", JSON.stringify({ WorkloadId: $scope.workloadId })).
					success(function (data, status, headers, config) {
						$scope.evaluationLoading = false;
						angular.forEach(data.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						$scope.evaluationChartData = data.Days;
						if (data.Days.length === 0) {
							$scope.noHistoricalDataForForecasting = true;
							$scope.queueStatisticsLoading = false;
							return;
						}

						var selectedMethod;
						if (data.ForecastMethodRecommended.Id === -1) {
							$scope.noHistoricalDataForEvaluation = true;
							selectedMethod = 1;
						} else {
							selectedMethod = data.ForecastMethodRecommended.Id;
						}
						//workload.selectedMethod = selectedMethod;
						$scope.ForecastMethodRecommended = data.ForecastMethodRecommended;

						if ($scope.isQueueStatisticsEnabled) {
							getQueueStatistics($scope.workloadId, selectedMethod);
						}
					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to do the evaluate." };
						$scope.evaluationLoading = false;
					});
			};
		}
	]);