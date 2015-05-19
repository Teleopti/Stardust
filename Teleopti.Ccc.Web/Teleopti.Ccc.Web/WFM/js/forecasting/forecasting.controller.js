'use strict';

angular.module('wfm.forecasting', [])
	.controller('ForecastingCtrl', [
		'$scope', '$state',
		function($scope, $state) {
			var startDate = moment().add(1, 'months').startOf('month').format("YYYY-MM-DD");
			var endDate = moment().add(2, 'months').startOf('month').format("YYYY-MM-DD");
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			$scope.nextStepAll = function(period) {
				$state.go('forecasting.runall', { period: period });
			};

			$scope.nextStepAdvanced = function(period) {
				$state.go('forecasting.target', { period: period });
			};
		}
	])
	.controller('ForecastingRunCtrl', [
		'$scope', '$stateParams', '$http',
		function($scope, $stateParams, $http) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			var workloads = [];
			angular.forEach($scope.targets, function(workload) {
				workloads.push({ WorkloadId: workload.Id, ForecastMethodId: workload.Method });
			});
			$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: workloads })).
				success(function(data, status, headers, config) {
					$scope.result = { success: true, message: 'You now have an updated forecast for the following workloads in your default scenario, based on last year\'s data:' };
				}).
				error(function(data, status, headers, config) {
					$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
				});
		}
	])
	.controller('ForecastingRunAllCtrl', [
		'$scope', '$stateParams', '$http', 'Forecasting',
		function ($scope, $stateParams, $http, forecasting) {

			$scope.period = $stateParams.period;
			$scope.targets = [];


			var forecastForOneSkill = function(index) {
				var skill = $scope.skills[index];
				if (!skill) {
					return;
				}
				var workloads = [];
				angular.forEach(skill.Workloads, function(workload) {
					workloads.push({ WorkloadId: workload.Id, ForecastMethodId: -1 });
				});

				angular.forEach(workloads, function(workload) {
					angular.forEach($scope.targets, function(target) {
						if (workload.WorkloadId === target.Id) {
							target.ShowProgress = true;
						}
					});
				});

				var findWorkloads =function(func) {
					angular.forEach(workloads, function (workload) {
						angular.forEach($scope.targets, function (target) {
							if (workload.WorkloadId === target.Id) {
								func(target);
							}
						});
					});
				}

				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: workloads })).
					success(function (data, status, headers, config) {
						findWorkloads(function (target) {
							target.IsSuccess = true;
						});
					}).
					error(function (data, status, headers, config) {
						findWorkloads(function (target) {
							target.IsFailed = false;
						});
					})
					.then(function() {
						forecastForOneSkill(++index);
						findWorkloads(function (target) {
							target.ShowProgress = false;
						});
					});
			}

			forecasting.skills.query().$promise.then(function (result) {
				$scope.skills = result;
				angular.forEach($scope.skills, function (skill) {
					angular.forEach(skill.Workloads, function(workload) {
						$scope.targets.push({ Id: workload.Id, Name: skill.Name + " / " + workload.Name, IsSuccess: false });
					});
				});

				forecastForOneSkill(0);
			});
		}
	]);