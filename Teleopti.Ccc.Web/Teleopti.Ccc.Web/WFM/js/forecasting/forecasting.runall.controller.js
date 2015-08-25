'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingRunAllCtrl', [
		'$scope', '$stateParams', '$http', 'Forecasting', '$state',
		function ($scope, $stateParams, $http, forecasting, $state) {

			$scope.period = $stateParams.period;
			$scope.targets = [];

			$scope.back = function () {
				$state.go('forecasting');
			};


			var forecastForOneSkill = function (index) {
				var skill = $scope.skills[index];
				if (!skill) {
					return;
				}
				var workloads = [];
				angular.forEach(skill.Workloads, function (workload) {
					workloads.push({ WorkloadId: workload.Id, ForecastMethodId: -1 });
				});

				var findWorkloads = function (func) {
					angular.forEach(workloads, function (workload) {
						angular.forEach($scope.targets, function (target) {
							if (workload.WorkloadId === target.Id) {
								func(target);
							}
						});
					});
				}

				findWorkloads(function (target) {
					target.ShowProgress = true;
				});

				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: workloads })).
					success(function (data, status, headers, config) {
						findWorkloads(function (target) {
							if (data.Success) {
								target.IsSuccess = true;
							}
							else {
								target.IsFailed = true;
								target.Message = data.Message;
							}
						});
					}).
					error(function (data, status, headers, config) {
						findWorkloads(function (target) {
							target.IsFailed = true;
							target.Message = 'Failed';
						});
					})
					.finally(function () {
						forecastForOneSkill(++index);
						findWorkloads(function (target) {
							target.ShowProgress = false;
						});
					});
			}

			forecasting.skills.query().$promise.then(function (result) {
				$scope.skills = result;
				angular.forEach($scope.skills, function (skill) {
					angular.forEach(skill.Workloads, function (workload) {
						$scope.targets.push({ Id: workload.Id, Name: skill.Name + " / " + workload.Name });
					});
				});

				forecastForOneSkill(0);
			});
		}
	]);