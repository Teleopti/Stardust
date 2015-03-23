'use strict';

angular.module('wfm.forecasting', [])
	.controller('ForecastingCtrl', [
		'$scope', '$state',
		function ($scope, $state) {
			var startDate = moment().add(1, 'months').startOf('month').toDate();
			var endDate = moment().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			$scope.nextStep = function (period) {
				$state.go('forecasting.target', { period: period }); //there's probably a better way to do that
			};
		}
	]
	)
	.controller('ForecastingTargetCtrl', [
		'$scope', '$stateParams', '$state', 'Forecasting',
		function ($scope, $stateParams, $state, Forecasting) {
			$scope.skillsDisplayed = [];
			$scope.all = { Name: 'All', Selected: false };

			$scope.toggleAll = function (selected) {
				$scope.all.Selected = !selected;
				angular.forEach($scope.skillsDisplayed, function(skill) {
					skill.Selected = !selected;
					angular.forEach(skill.Workloads, function (workload) {
						workload.Selected = !selected;
					});
				});
			};

			$scope.toggleSkill = function (skill) {
				if (skill.Selected) {
					skill.Selected = false;
					angular.forEach(skill.Workloads, function(workload) {
						workload.Selected = false;
					});
				} else {
					skill.Selected = true;
					angular.forEach(skill.Workloads, function (workload) {
						workload.Selected = true;
					});
				}
			};

			$scope.toggleWorkload = function (workload) {
				if (workload.Selected) {
					workload.Selected = false;
				} else {
					workload.Selected = true;
				}
			};

			Forecasting.skills.query().$promise.then(function (result) {
				$scope.skillsDisplayed = result;
			});

			Forecasting.accuracyResult.query().$promise.then(function (workloadAccuracies) {
				var sum = 0;
				angular.forEach($scope.skillsDisplayed, function (skill) {
					var sumForSkill = 0;
					angular.forEach(skill.Workloads, function (w) {
						angular.forEach(workloadAccuracies, function (workload) {
							if (workload.WorkloadId === w.Id) {
								w.Accuracy = workload.Accuracy + '%';
								sumForSkill += workload.Accuracy;
								sum += workload.Accuracy;
							}
						});
					});
					skill.Accuracy = (sumForSkill / skill.Workloads.length).toFixed(1) + '%';
				});

				$scope.all.Accuracy = (sum / workloadAccuracies.length).toFixed(1) + '%';
			});

			$scope.targets = function () {
				var result = [];
				angular.forEach($scope.skillsDisplayed, function (skill) {
					angular.forEach(skill.Workloads, function (workload) {
						if (workload.Selected)
							result.push(workload.Id);
					});
				});
				return result;
			};

			$scope.nextStep = function () {
				$state.go('forecasting.run', { period: $stateParams.period, targets: $scope.targets() });
			};
		}]
		)
	.controller('ForecastingRunCtrl', ['$scope', '$stateParams', '$http',
		function ($scope, $stateParams, $http) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: $scope.targets })).
				success(function (data, status, headers, config) {
					$scope.result = { success: true, message: 'You now have an updated forecast in your default scenario, based on last year\'s data.' };
				}).
				error(function (data, status, headers, config) {
					$scope.result = { success: false, message: 'The forecast has failed. Please try again later' };
				});
		}]
);