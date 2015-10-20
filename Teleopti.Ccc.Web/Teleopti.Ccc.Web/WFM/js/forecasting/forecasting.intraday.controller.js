(function () {
	'use strict';

	angular.module('wfm.forecasting')
		.controller('ForecastingIntradayCtrl', ['$scope', '$state', '$stateParams', '$http',
			function ($scope, $state, $stateParams, $http) {
				$scope.workloadId = $stateParams.workloadId;
				$scope.workloadName = "";
				$scope.weekDays = [];

				$scope.back = function () {
					$state.go("forecasting-target", { period: $stateParams.period });
				};

				var loadIntradayPattern = function () {
					$scope.loading = true;
					$http.post("../api/Forecasting/IntradayPattern", JSON.stringify({ WorkloadId: $scope.workloadId })).
						success(function (data, status, headers, config) {

							$scope.loading = false;
							$scope.workloadName = data.WorkloadName;

							angular.forEach(data.WeekDays, function (weekDay) {
								weekDay.ChartId = "chartIntraday" + weekDay.DayOfWeek;
							});
							$scope.weekDays = data.WeekDays;

							angular.element(document).ready(function () {
								angular.forEach($scope.weekDays, function (weekDay) {
									weekDay.Tasks.unshift("Tasks");
									var chart = c3.generate({
										bindto: "#" + weekDay.ChartId,
										data: {
											columns: [
												weekDay.Tasks
											],
											type: 'bar'
										},
										axis: {
											x: {
												type: 'category',
												categories: weekDay.Periods,
												tick: {
													fit: false,
													culling: true
												}
											}
										},
										legend: {
											hide: true
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

				loadIntradayPattern();
			}
		]);
})();