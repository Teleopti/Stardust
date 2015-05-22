'use strict';

angular.module('wfm.forecasting.target', ['gridshore.c3js.chart'])
	.controller('ForecastingTargetCtrl', [
			'$scope', '$stateParams', '$state', 'Forecasting', '$http',
			function($scope, $stateParams, $state, forecasting, $http) {
				$scope.showSelection = true;
				$scope.skillsDisplayed = [];
				$scope.all = { Name: 'All', Selected: false, show: true, numberOfSelectedWorkloads: 0 };
				$scope.showExplaination = false;
				$scope.selectedIds = [];


				var methodNames = ["Teleopti Classic", "Teleopti Classic with Trend"];
				$scope.dataColumns = [{ id: "vh", type: "line", name: "Queue Statistics" },
									{ id: "vb", type: "line", name: "Forecast Method" }];
				$scope.dataX = { id: "date" };

				$scope.openModal = function (workload) {
					workload.modalLaunch = true;
					workload.noHistoricalDataForEvaluation = false;
					workload.noHistoricalDataForForecasting = false;
					workload.loaded = false;
					
					$http.post("../api/Forecasting/Evaluate", JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, WorkloadId: workload.Id })).
						success(function (data, status, headers, config) {
							workload.loaded = true;
							angular.forEach(data.ForecastDays, function(day) {
								day.date = new Date(Date.parse(day.date));
							});
							workload.chartData = data.ForecastDays;
							if (data.ForecastDays.length === 0) {
								workload.noHistoricalDataForForecasting = true;
								return;
							}

							var selectedMethod;
							if (data.ForecastMethodRecommended === -1) {
								workload.noHistoricalDataForEvaluation = true;
								selectedMethod = 0;
							} else {
								selectedMethod = data.ForecastMethodRecommended;
							}
							workload.selectedMethod = selectedMethod;
							$scope.dataColumns[1].name = "Forecast Method";
						}).
						error(function(data, status, headers, config) {
							$scope.error = { message: "Failed to do the evaluate." };
							workload.loaded = true;
						});
				};

				$scope.cancelMethod = function (workload) {
					workload.modalLaunch = false;
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
							workload.chartId = "chart" + workload.Id;
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