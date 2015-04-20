'use strict';

angular.module('wfm.forecasting.target', ['n3-line-chart'])
	.controller('ForecastingTargetCtrl', [
			'$scope', '$stateParams', '$state', 'Forecasting', '$http',
			function($scope, $stateParams, $state, Forecasting, $http) {
				$scope.showSelection = true;
				$scope.skillsDisplayed = [];
				$scope.all = { Name: 'All', Selected: false, show: true, numberOfSelectedWorkloads: 0 };
				$scope.showExplaination = false;
				$scope.selectedIds = [];

				$scope.data = [];

				$scope.options = {
					lineMode: "cardinal",
					tension: 1.0,
					axes: { x: { type: "date", key: "date" }, y: { type: "linear" } },
					tooltipMode: "dots",
					drawLegend: true,
					drawDots: true,
					stacks: [],
					series: [
					  {
					  	y: "vh",
					  	label: "Historical data",
					  	type: "line",
					  	color: "#ee8f7d",
					  	axis: "y",
					  	thickness: "1px",
					  	visible: true,
					  	dotSize: 2,
					  	id: "series_0",
					  	drawDots: true
					  },
					  {
					  	y: "v0",
					  	label: "Teleopti Classic",
					  	type: "line",
					  	color: "#66c2ff",
					  	axis: "y",
					  	visible: true,
					  	id: "series_1",
					  	thickness: "1px",
					  	dotSize: 2,
					  	drawDots: true
					  },
					  {
					  	y: "v1",
					  	label: "Teleopti Classic with Trend",
					  	color: "#67c285",
					  	axis: "y",
					  	type: "line",
					  	thickness: "1px",
					  	visible: true,
					  	dotSize: 2,
					  	id: "series_2",
					  	drawDots: true
					  }
					],
					tooltip: { mode: "axes", interpolate: true }
				};

				$scope.openModal = function(workload) {
					workload.modalLaunch = true;
					$scope.data = [];
					$http.post("../api/Forecasting/PreForecast", JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate , WorkloadId: workload.Id})).
						success(function(data, status, headers, config) {
							angular.forEach(data.ForecastDays, function(day) {
								day.date = new Date(Date.parse(day.date));

							});
							$scope.data = data.ForecastDays;
							$scope.options.series[data.ForecastMethodRecommended + 1].thickness = 4;
						}).
						error(function(data, status, headers, config) {
							console.log(data);
							$scope.error = { message: "Failed to do the preforecast." };
						});
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
					});
				});

				$scope.targets = function() {
					var result = [];
					angular.forEach($scope.skillsDisplayed, function (skill) {
						angular.forEach(skill.Workloads, function(workload) {
							if (workload.Selected)
								result.push({ Id: workload.Id, Name: workload.Name });
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