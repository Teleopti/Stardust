﻿'use strict';

angular.module('wfm.forecasting.dev', ['gridshore.c3js.chart'])
	.controller('ForecastingDevCtrl', [
		'$scope', '$state', '$stateParams', '$http',
		function ($scope, $state, $stateParams, $http) {
			$scope.workloadId = $stateParams.workloadId;
			$scope.workloadName = "";
			$scope.methods = [];
			$scope.chartDataColumns = [
				{ id: "vh", type: "line", name: "Queue Statistics" },
				{ id: "vb", type: "line", name: "Forecast Method" }
			];
			$scope.dataX = { id: "date" };

			$scope.loading = true;
			$http.post("../api/Forecasting/EvaluateDev", JSON.stringify({ WorkloadId: $scope.workloadId })).
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
				}).
				error(function(data, status, headers, config) {
					$scope.error = { message: "Failed to do the evaluate dev." };
					$scope.loading = false;
				});
		}
	]);