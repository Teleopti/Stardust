'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingAdvancedCtrl', ['$scope', '$state', '$stateParams', '$http', 'Toggle', 'Forecasting',
		function ($scope, $state, $stateParams, $http, toggleService, forecasting) {
			var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
			var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month
			$scope.workloadId = $stateParams.workloadId;

			$scope.isForecastRunning = false;
			forecasting.status.get().$promise.then(function (result) {
				$scope.isForecastRunning = result.IsRunning;
			});

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

			$scope.nextStepOneWorkload = function () {
				if ($scope.disableNextStepOneWorkload()) {
					return;
				}
				$state.go("forecasting", { period: $scope.period, target: $scope.workloadId, running: true });
			};

			$scope.modalLaunch = false;
			$scope.displayModal = function () {
				$scope.modalLaunch = true;
			};
			$scope.cancelModal = function () {
				$scope.modalLaunch = false;
			};

			$scope.moreThanOneYear = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					var dateDiff = new Date($scope.period.endDate - $scope.period.startDate);
					dateDiff.setDate(dateDiff.getDate() - 1);
					return dateDiff.getFullYear() - 1970 >= 1;
				} else
					return false;
			};

			$scope.disableNextStepOneWorkload = function () {
				return $scope.moreThanOneYear() || $scope.isForecastRunning;
			};

			$scope.setRangeClass = function (date, mode) {
				if (mode === 'day') {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var startDay = new Date($scope.period.startDate).setHours(12, 0, 0, 0);
					var endDay = new Date($scope.period.endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= startDay && dayToCheck <= endDay) {
						return 'in-date-range';
					}
				}
				return '';
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