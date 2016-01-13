﻿(function () {
	'use strict';

	angular.module('wfm.forecasting')
		.controller('ForecastingAdvancedCtrl', ['$scope', '$state', '$stateParams', '$http', 'Toggle', '$translate', 'forecastingService',
			function ($scope, $state, $stateParams, $http, toggleService, $translate, forecastingService) {
				$scope.workloadId = $stateParams.workloadId;
				$scope.workloadName = $stateParams.workloadName;

				$scope.chartInfo = {
					evaluationChartDataColumns: [
						{ id: "vh", type: "line", name: $translate.instant('ResReportQueueStatistics') },
						{ id: "vb", type: "line", name: $translate.instant('ForecastMethod') }
					],
					queueStatisticsChartDataColumns: [
						{ id: "vh", type: "line", name: $translate.instant('ResReportQueueStatistics') },
						{ id: "vh2", type: "line", name: $translate.instant('QueueStatWoOutliers') }
					],
					dataX: { id: "date" }
				};

				$scope.back = function () {
					$state.go("forecasting.start");
				};

				$scope.isQueueStatisticsEnabled = toggleService.WfmForecast_QueueStatistics_32572;

				var getQueueStatistics = function(workloadId, methodId) {
					$scope.queueStatisticsLoading = true;
					forecastingService.queueStatistics(JSON.stringify({ WorkloadId: workloadId, MethodId: methodId }),
						function(data, status, headers, config) {
							$scope.queueStatisticsLoading = false;
							angular.forEach(data.QueueStatisticsDays, function(day) {
								day.date = new Date(Date.parse(day.date));
							});
							$scope.queueStatisticsChartData = data.QueueStatisticsDays;
						}, function(data, status, headers, config) {
							$scope.error = { message: "Failed to get queue statisctics." };
							$scope.queueStatisticsLoading = false;
						});
				};

				$scope.init = function () {
					$scope.noHistoricalDataForEvaluation = false;
					$scope.noHistoricalDataForForecasting = false;
					$scope.evaluationLoading = true;
					$scope.queueStatisticsLoading = true;
					$scope.evaluationChartData = [];
					$scope.queueStatisticsChartData = [];

					forecastingService.evaluate(JSON.stringify({ WorkloadId: $scope.workloadId }),
						function(data, status, headers, config) {
							$scope.evaluationLoading = false;
							angular.forEach(data.Days, function(day) {
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
							$scope.selectedMethod = selectedMethod;
							$scope.ForecastMethodRecommended = data.ForecastMethodRecommended;

							if ($scope.isQueueStatisticsEnabled) {
								getQueueStatistics($scope.workloadId, selectedMethod);
							}
						}, function(data, status, headers, config) {
							$scope.error = { message: "Failed to do the evaluate." };
							$scope.evaluationLoading = false;
							$scope.queueStatisticsLoading = false;
						}
					);
				};
			}
		]);
})();
