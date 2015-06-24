'use strict';

angular.module('wfm.forecasting', [])
	.controller('ForecastingCtrl', [
		'$scope', '$state',
		function($scope, $state) {
			var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
			var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			$scope.moreThanTwoYears = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					var dateDiff = new Date($scope.period.endDate - $scope.period.startDate);
					dateDiff.setDate(dateDiff.getDate() - 1);
					return dateDiff.getFullYear() - 1970 >= 2;
				}
				else
					return false;
			};

			$scope.nextStepAll = function(period) {
				$state.go('forecasting.runall', { period: period });
			};

			$scope.nextStepAdvanced = function(period) {
				$state.go('forecasting.target', { period: period });
			};

			$scope.setRangeClass = function (date, mode) {
				if (mode === 'day') {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var startDay = new Date($scope.period.startDate).setHours(12, 0, 0, 0);
					var endDay = new Date($scope.period.endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= startDay && dayToCheck < endDay) {
						return 'range';
					}
				}
				return '';
			};
		}
	])
	.controller('ForecastingRunCtrl', [
		'$scope', '$stateParams', '$http', 'Forecasting',
		function ($scope, $stateParams, $http, forecasting) {

			$scope.period = $stateParams.period;
			$scope.targets = $stateParams.targets;
			var workloads = [];
			angular.forEach($scope.targets, function (workload) {
				workloads.push({ WorkloadId: workload.Id, ForecastMethodId: workload.Method });
			});

			$scope.modalInfo = {
				resultChartDataColumns: [
					{ id: "vc", type: "line", name: "Calls" },
					{ id: "vaht", type: "bar", name: "Talk time" },
					{ id: "vacw", type: "bar", name: "ACW" }
				],
				dataX: { id: "date" }
			};

			$scope.isResultViewEnabled = false;
			forecasting.isToggleEnabled.query({ toggle: 'WfmForecast_ResultView_33605' }).$promise.then(function (result) {
				$scope.isResultViewEnabled = result.IsEnabled;
			});

			$scope.openModal = function (workload) {
				$scope.modalInfo.workloadName = workload.Name;
				$scope.modalInfo.modalLaunch = true;
				$scope.modalInfo.loaded = false;

				$http.post("../api/Forecasting/ForecastResult", JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, WorkloadId: workload.Id })).
					success(function (data, status, headers, config) {
						angular.forEach(data.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						$scope.modalInfo.resultChartData = data.Days;
						$scope.modalInfo.loaded = true;
					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to get forecast result." };
						$scope.modalInfo.loaded = true;
					});
			};

			$scope.cancelMethod = function () {
				$scope.modalInfo.modalLaunch = false;
			};

			var forecastForOneWorkload = function(index) {
				var workload = workloads[index];
				if (!workload) {
					return;
				}

				var findWorkload = function (func) {
					angular.forEach($scope.targets, function (target) {
						if (workload.WorkloadId === target.Id) {
							func(target);
						}
					});
				}

				findWorkload(function (target) {
					target.ShowProgress = true;
				});

				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: [workload] })).
					success(function (data, status, headers, config) {
						findWorkload(function (target) {
							target.IsSuccess = true;
						});
					}).
					error(function (data, status, headers, config) {
						findWorkload(function (target) {
							target.IsFailed = true;
						});
					})
					.finally(function () {
						forecastForOneWorkload(++index);
						findWorkload(function (target) {
							target.ShowProgress = false;
						});
					});
			};

			forecastForOneWorkload(0);
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

				var findWorkloads =function(func) {
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
							target.IsSuccess = true;
						});
					}).
					error(function (data, status, headers, config) {
						findWorkloads(function (target) {
							target.IsFailed = true;
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
					angular.forEach(skill.Workloads, function(workload) {
						$scope.targets.push({ Id: workload.Id, Name: skill.Name + " / " + workload.Name });
					});
				});

				forecastForOneSkill(0);
			});
		}
	]);