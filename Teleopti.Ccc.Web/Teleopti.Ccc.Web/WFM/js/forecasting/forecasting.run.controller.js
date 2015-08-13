'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingRunCtrl', [
		'$scope', '$stateParams', '$http', 'Forecasting', '$state', 'Toggle',
		function ($scope, $stateParams, $http, forecasting, $state, toggleService) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			var workloads = [];
			angular.forEach($scope.targets, function (workload) {
				workloads.push({ WorkloadId: workload.Id, ForecastMethodId: workload.Method });
			});

			$scope.modalInfo = {
				resultChartDataColumns: [
					{ id: "vc", type: "line", name: "Calls" },
					{ id: "vaht", type: "bar", name: "Talk time" },
					{ id: "vacw", type: "bar", name: "ACW" }
				],
				dataX: { id: "date" }
			};

			$scope.isResultViewEnabled = false;
			toggleService.isFeatureEnabled.query({ toggle: 'WfmForecast_ResultView_33605' }).$promise.then(function (result) {
				$scope.isResultViewEnabled = result.IsEnabled;
			});

			$scope.openModal = function (workload) {
				$scope.modalInfo.workloadName = workload.Name;
				$scope.modalInfo.modalLaunch = true;
				$scope.modalInfo.loaded = false;

				$http.post("../api/Forecasting/ForecastResult", JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, WorkloadId: workload.Id })).
					success(function (data, status, headers, config) {
						angular.forEach(data.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						$scope.modalInfo.resultChartData = data.Days;
						$scope.modalInfo.loaded = true;
					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to get forecast result." };
						$scope.modalInfo.loaded = true;
					});
			};

			$scope.cancelMethod = function () {
				$scope.modalInfo.modalLaunch = false;
			};

			$scope.back = function () {
				$state.go('forecasting-target', { period: $scope.period });
			};

			var forecastForOneWorkload = function (index) {
				var workload = workloads[index];
				if (!workload) {
					return;
				}

				var findWorkload = function (func) {
					angular.forEach($scope.targets, function (target) {
						if (workload.WorkloadId === target.Id) {
							func(target);
						}
					});
				}

				findWorkload(function (target) {
					target.ShowProgress = true;
				});

				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: [workload] })).
					success(function (data, status, headers, config) {
						findWorkload(function (target) {
							if (data.Success) {
								target.IsSuccess = true;
							} else {
								target.IsFailed = true;
								target.Message = data.Message;
							}
						});
					}).
					error(function (data, status, headers, config) {
						findWorkload(function (target) {
							target.IsFailed = true;
						});
					})
					.finally(function () {
						forecastForOneWorkload(++index);
						findWorkload(function (target) {
							target.ShowProgress = false;
						});
					});
			};

			forecastForOneWorkload(0);
		}
	]);