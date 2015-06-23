﻿'use strict';

angular.module('wfm.forecasting.target', ['gridshore.c3js.chart'])
	.controller('ForecastingTargetCtrl', [
			'$scope', '$stateParams', '$state', 'Forecasting', '$http',
			function($scope, $stateParams, $state, forecasting, $http) {
				$scope.showSelection = true;
				$scope.skillsDisplayed = [];
				$scope.all = { Name: 'All', Selected: false, show: true, numberOfSelectedWorkloads: 0 };
				$scope.showExplaination = false;
				$scope.selectedIds = [];
				$scope.modalInfo = {};

				$scope.dataColumns = [{ id: "vh", type: "line", name: "Queue Statistics" },
									{ id: "vb", type: "line", name: "Forecast Method" }];

				$scope.dataColumns2 = [{ id: "vh", type: "line", name: "Queue Statistics" },
									{ id: "vh2", type: "line", name: "Queue Statistics without Outliers" }];

				$scope.dataX = { id: "date" };
				$scope.isQueueStatisticsEnabled = false;
				forecasting.isToggleEnabled.query({ toggle: 'WfmForecast_QueueStatistics_32572' }).$promise.then(function (result) {
					$scope.isQueueStatisticsEnabled = result.IsEnabled;
				});

				var getQueueStatistics = function (workloadId, methodId) {
					$scope.modalInfo.queueStatisticsLoading = true;
					$http.post("../api/Forecasting/QueueStatistics", JSON.stringify({ WorkloadId: workloadId, MethodId: methodId })).
						success(function (data, status, headers, config) {
							$scope.modalInfo.queueStatisticsLoading = false;
							angular.forEach(data.QueueStatisticsDays, function (day) {
								day.date = new Date(Date.parse(day.date));
							});
							$scope.modalInfo.chartData2 = data.QueueStatisticsDays;
						}).
						error(function (data, status, headers, config) {
							$scope.error = { message: "Failed to get queue statisctics." };
							$scope.modalInfo.queueStatisticsLoading = false;
						});
				};

				$scope.openModal = function (workload) {
					$scope.modalInfo.workloadName = workload.Name;
					$scope.modalInfo.modalLaunch = true;
					$scope.modalInfo.noHistoricalDataForEvaluation = false;
					$scope.modalInfo.noHistoricalDataForForecasting = false;
					$scope.modalInfo.evaluationLoading = true;
					
					$http.post("../api/Forecasting/Evaluate", JSON.stringify({ WorkloadId: workload.Id })).
						success(function (data, status, headers, config) {
							$scope.modalInfo.evaluationLoading = false;
							angular.forEach(data.Days, function(day) {
								day.date = new Date(Date.parse(day.date));
							});
							$scope.modalInfo.chartData = data.Days;
							if (data.Days.length === 0) {
								$scope.modalInfo.noHistoricalDataForForecasting = true;
								return;
							}

							var selectedMethod;
							if (data.ForecastMethodRecommended.Id === -1) {
								$scope.modalInfo.noHistoricalDataForEvaluation = true;
								selectedMethod = 1;
							} else {
								selectedMethod = data.ForecastMethodRecommended.Id;
							}
							workload.selectedMethod = selectedMethod;
							$scope.modalInfo.ForecastMethodRecommended = data.ForecastMethodRecommended;

							if ($scope.isQueueStatisticsEnabled) {
								getQueueStatistics(workload.Id, selectedMethod);
							}
						}).
						error(function(data, status, headers, config) {
							$scope.error = { message: "Failed to do the evaluate." };
							$scope.modalInfo.evaluationLoading = false;
						});
				};

				$scope.cancelMethod = function () {
					$scope.modalInfo.modalLaunch = false;
				};

				$scope.hasOneSelected = function() {
					return $scope.all.numberOfSelectedWorkloads !== 0;
				};

				$scope.toggleAll = function(selected) {
					$scope.all.Selected = !selected;
					angular.forEach($scope.skillsDisplayed, function(skill) {
						skill.Selected = !selected;
						angular.forEach(skill.Workloads, function(workload) {
							workload.Selected = !selected;
						});
					});
				};

				$scope.toggleSkill = function (skill) {
					skill.Selected = !skill.Selected;
					
					angular.forEach(skill.Workloads, function (workload) {
						workload.Selected = skill.Selected;
					});
				};

				$scope.toggleWorkload = function (workload) {
					workload.Selected = !workload.Selected;
				};

				$scope.$watch("skillsDisplayed", function() {
					var allSet = true;
					$scope.all.numberOfSelectedWorkloads = 0;
					$scope.all.totalWorkloads = 0;
					angular.forEach($scope.skillsDisplayed, function(skill) {
						var allSetForSkill = true;
						skill.numberOfSelectedWorkloads = 0;
						$scope.all.totalWorkloads += skill.Workloads.length;
						angular.forEach(skill.Workloads, function(workload) {
							if (!workload.Selected) {
								allSetForSkill = false;
								allSet = false;
							} else {
								skill.numberOfSelectedWorkloads++;
								$scope.all.numberOfSelectedWorkloads++;
							}
						});
						skill.Selected = allSetForSkill;
					});
					$scope.all.Selected = allSet;
				}, true);

				forecasting.skills.query().$promise.then(function (result) {
					$scope.skillsDisplayed = result;
					angular.forEach($scope.skillsDisplayed, function (skill) {
						skill.show = true;
						angular.forEach(skill.Workloads, function (workload) {
							workload.selectedMethod = -1;
						});
					});
				});

				$scope.targets = function() {
					var result = [];
					angular.forEach($scope.skillsDisplayed, function (skill) {
						angular.forEach(skill.Workloads, function(workload) {
							if (workload.Selected)
								result.push({ Id: workload.Id, Name: skill.Name + " / " + workload.Name, Method: workload.selectedMethod });
						});
					});
					return result;
				};

				$scope.nextStep = function () {
					if ($scope.hasOneSelected())
						$state.go("forecasting.run", { period: $stateParams.period, targets: $scope.targets() });
				};

				$scope.back = function () {
					$state.go("forecasting");
				};
			}
		]
	);