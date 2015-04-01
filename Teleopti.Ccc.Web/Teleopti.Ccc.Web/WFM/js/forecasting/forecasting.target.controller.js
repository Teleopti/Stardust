'use strict';

angular.module('wfm.forecasting.target', [])
	.controller('ForecastingTargetCtrl', [
			'$scope', '$stateParams', '$state', 'Forecasting',
			function($scope, $stateParams, $state, Forecasting) {
				$scope.showSelection = true;
				$scope.skillsDisplayed = [];
				$scope.all = { Name: 'All', Selected: false, show: true };
				$scope.showExplaination = false;
				$scope.hasOneSelected = false;
				$scope.selectedIds = [];
				

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

				$scope.$watch('skillsDisplayed', function() {
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

				Forecasting.measureForecastMethod.query().$promise.then(function (result) {
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
								result.push({ Id: workload.Id, Name: workload.Name});
						});
					});
					$scope.hasOneSelected = (result.length > 0);
					return result;
				};

				$scope.nextStep = function() {
					$state.go('forecasting.run', { period: $stateParams.period, targets: $scope.targets() });
				};
			}
		]
	);