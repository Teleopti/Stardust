'use strict';

angular.module('wfm.forecasting.target', ['gridshore.c3js.chart'])
	.controller('ForecastingTargetCtrl', [
			'$scope', '$stateParams', '$state', 'Forecasting', '$http',
			function($scope, $stateParams, $state, Forecasting, $http) {
				$scope.showSelection = true;
				$scope.skillsDisplayed = [];
				$scope.all = { Name: 'All', Selected: false, show: true, numberOfSelectedWorkloads: 0 };
				$scope.showExplaination = false;
				$scope.selectedIds = [];

				$scope.dataColumns = [{ id: "vh", type: "line", name: "Historical data" },
									{ id: "v0", type: "line", name: "Teleopti Classic" },
									{ id: "v1", type: "line", name: "Teleopti Classic with Trend" }];
				$scope.dataX = { id: "date" };

				$scope.openModal = function (workload) {
					workload.modalLaunch = true;
					workload.noHistoricalDataForEvaluation = false;
					workload.noHistoricalDataForForecasting = false;
					workload.loaded = false;

					
					
					$http.post("../api/Forecasting/PreForecast", JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate , WorkloadId: workload.Id})).
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

							angular.forEach(data.ForecastMethods, function (method) {
								method.DomId = workload.Id + (method.ForecastMethodType + 1);
								method.Name = $scope.dataColumns[method.ForecastMethodType + 1].name;
							});

							workload.forecastMethods = data.ForecastMethods;

							var selectedMethod;
							if (workload.methodToUse !== -1) {
								selectedMethod = workload.methodToUse;
							} else {
								if (data.ForecastMethodRecommended === -1) {
									workload.noHistoricalDataForEvaluation = true;
									selectedMethod = 0;
								} else {
									selectedMethod = data.ForecastMethodRecommended;
								}
							}
							workload.selectedMethod = selectedMethod;
						}).
						error(function(data, status, headers, config) {
							console.log(data);
							$scope.error = { message: "Failed to do the preforecast." };
							workload.loaded = true;
						});
				};

				$scope.saveMethod = function (workload) {
					workload.methodToUse = workload.selectedMethod;
					workload.modalLaunch = false;
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

				Forecasting.skills.query().$promise.then(function (result) {
					$scope.skillsDisplayed = result;
					angular.forEach($scope.skillsDisplayed, function (skill) {
						skill.show = true;
						angular.forEach(skill.Workloads, function (workload) {

							workload.chartId = "chart" + workload.Id;
							workload.methodToUse = -1;

							workload.methodChanged = function (newMethod) {
								// do something
							};
						});
					});
				});

				$scope.targets = function() {
					var result = [];
					angular.forEach($scope.skillsDisplayed, function (skill) {
						angular.forEach(skill.Workloads, function(workload) {
							if (workload.Selected)
								result.push({ Id: workload.Id, Name: skill.Name + " / " + workload.Name, Method: workload.methodToUse });
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