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
			$scope.showSelection = false;
			$scope.skillsDisplayed = [];
			$scope.all = { Name: 'All', Selected: false };

			$scope.toggleAll = function (selected) {
				$scope.all.Selected = !selected;
				angular.forEach($scope.skillsDisplayed, function (skill) {
					skill.Selected = !selected;
					angular.forEach(skill.Workloads, function (workload) {
						workload.Selected = !selected;
					});
				});
			};

			$scope.toggleSkill = function (skill) {
				var isSelected = skill.Selected;
				skill.Selected = !isSelected;
				angular.forEach(skill.Workloads, function (workload) {
					workload.Selected = !isSelected;
				});
			};

			$scope.toggleWorkload = function (workload) {
				var isSelected = workload.Selected;
				workload.Selected = !isSelected;
			};

			$scope.$watch('skillsDisplayed', function () {
				var allSet = true;
				angular.forEach($scope.skillsDisplayed, function (skill) {
					var allSetForSkill = true;
					angular.forEach(skill.Workloads, function (workload) {
						if (!workload.Selected) {
							allSetForSkill = false;
							allSet = false;
						}
					});
					if (allSetForSkill) {
						skill.Selected = true;
					} else {
						skill.Selected = false;
					}
				});

				if (allSet) {
					$scope.all.Selected = true;
				} else {
					$scope.all.Selected = false;
				}
			}, true);

			$scope.showExplaination = false;

			Forecasting.skills.query().$promise.then(function (result) {
				$scope.skillsDisplayed = result;
			});

			Forecasting.accuracyResult.query().$promise.then(function (workloadAccuracies) {
				var sum = 0;
				var numberOfWorkloadsToSkip = 0;
				angular.forEach($scope.skillsDisplayed, function (skill) {
					var sumForSkill = 0;
					var numberToSkipForThisSkill = 0;
					angular.forEach(skill.Workloads, function (w) {
						angular.forEach(workloadAccuracies, function (workload) {
							if (workload.WorkloadId === w.Id) {
								if (workload.Accuracy === 'NaN') {
									numberOfWorkloadsToSkip++;
									numberToSkipForThisSkill++;
									w.Accuracy = '-%';
								} else {
									w.Accuracy = workload.Accuracy + '%';
									sumForSkill += workload.Accuracy;
									sum += workload.Accuracy;
								}
							}
						});
					});
					if (skill.Workloads.length - numberToSkipForThisSkill === 0) {
						skill.Accuracy = '-%';
					} else {
						skill.Accuracy = (sumForSkill / (skill.Workloads.length - numberToSkipForThisSkill)).toFixed(1) + '%';
					}
				});
				if (workloadAccuracies.length - numberOfWorkloadsToSkip === 0) {
					$scope.all.Accuracy = '-%';
				} else {
					$scope.all.Accuracy = (sum / (workloadAccuracies.length - numberOfWorkloadsToSkip)).toFixed(1) + '%';
				}
				$scope.showExplaination = (numberOfWorkloadsToSkip !== 0);
				$scope.showSelection = true;
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