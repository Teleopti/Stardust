'use strict';

angular.module('wfm.forecasting.method', ['gridshore.c3js.chart'])
	.controller('ForecastingMethodCtrl', [
		'$scope', '$state', '$stateParams', '$http',
		function ($scope, $state, $stateParams, $http) {
			$scope.workloadId = $stateParams.workloadId;
			$scope.workloadName = "";
			$scope.methods = [];
			$scope.weekDays = [];
			$scope.chartDataColumns = [
				{ id: "vh", type: "line", name: "Queue Statistics" },
				{ id: "vb", type: "line", name: "Forecast Method" }
			];
			$scope.dataX = { id: "date" };

			var loadIntradayPattern = function() {
				$http.post("../api/Forecasting/IntradayPattern", JSON.stringify({ WorkloadId: $scope.workloadId })).
				success(function (data, status, headers, config) {

						angular.forEach(data.WeekDays, function(weekDay) {
							weekDay.ChartId = "chartIntraday" + weekDay.DayOfWeek;
						});
						$scope.weekDays = data.WeekDays;

						angular.element(document).ready(function () {

							angular.forEach($scope.weekDays, function(weekDay) {
								weekDay.Tasks.unshift("Tasks");
								var chart = c3.generate({
									bindto: "#"+ weekDay.ChartId,
									data: {
										columns: [
											weekDay.Tasks
										],
										type: 'bar'
									},
									axis: {
										x: {
											type: 'category',
											categories: weekDay.Periods
										}
									},
									bar: {
										width: {
											ratio: 1
										}
									}
								});
							});

					});
				}).
				error(function (data, status, headers, config) {
					$scope.error = { message: "Failed to calculate the intraday pattern." };
					$scope.loading = false;
				});
			};

			$scope.loading = true;
			$http.post("../api/Forecasting/EvaluateMethods", JSON.stringify({ WorkloadId: $scope.workloadId })).
				success(function(data, status, headers, config) {
					$scope.loading = false;
					$scope.workloadName = data.WorkloadName;

					angular.forEach(data.Methods, function (method) {
						method.ChartId = "chart" + method.MethodId;
						angular.forEach(method.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
					});
					$scope.methods = data.Methods;

					loadIntradayPattern();
				}).
				error(function(data, status, headers, config) {
					$scope.error = { message: "Failed to do the methods evaluation." };
					$scope.loading = false;
				});
		}
	]);