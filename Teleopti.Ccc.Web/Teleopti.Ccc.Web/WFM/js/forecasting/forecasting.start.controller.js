'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingStartCtrl', [
		'$scope', '$state', 'Forecasting', '$http', '$stateParams',
		function ($scope, $state, forecasting, $http, $stateParams) {
			if ($stateParams.period) {
				$scope.period = $stateParams.period;
			} else {
				var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
				var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
				$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month
			}

			$scope.isForecastRunning = false;
			forecasting.status.get().$promise.then(function (result) {
				$scope.isForecastRunning = result.IsRunning;
			});

			$scope.workloads = [];
			forecasting.skills.query().$promise.then(function (result) {
				$scope.skills = result;
				angular.forEach($scope.skills, function (skill) {
					angular.forEach(skill.Workloads, function (workload) {
						$scope.workloads.push({ Id: workload.Id, Name: skill.Name + " - " + workload.Name, ChartId: "chart"+workload.Id });
					});
				});
			});

			$scope.modalLaunch = false;
			$scope.displayModal = function() {
				$scope.modalLaunch = true;
			};
			$scope.cancelModal = function () {
				$scope.modalLaunch = false;
			};

			$scope.chartInfo = {
				resultChartDataColumns: [
					{ id: "vc", type: "line", name: "Calls" },
					{ id: "vaht", type: "bar", name: "Talk time" },
					{ id: "vacw", type: "bar", name: "ACW" }
				],
				dataX: { id: "date" }
			};


			$scope.getForecastResult = function(workload) {
				workload.forecastResultLoaded = false;

				$scope.resultChartData = [];
				var resultStartDate = moment().utc().add(1, 'days');
				var resultEndDate = moment(resultStartDate).add(1, 'months');
				$http.post("../api/Forecasting/ForecastResult", JSON.stringify({ ForecastStart: resultStartDate.toDate(), ForecastEnd: resultEndDate.toDate(), WorkloadId: workload.Id })).
					success(function (data, status, headers, config) {
						angular.forEach(data.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						workload.resultChartData = data.Days;
						workload.forecastResultLoaded = true;
					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to get forecast result." };
						workload.forecastResultLoaded = true;
					});
			};

			$scope.moreThanOneYear = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					var dateDiff = new Date($scope.period.endDate - $scope.period.startDate);
					dateDiff.setDate(dateDiff.getDate() - 1);
					return dateDiff.getFullYear() - 1970 >= 1;
				} else
					return false;
			};

			$scope.updateStartDate = function () {
				$scope.period.startDate = angular.copy($scope.period.startDate);
			};

			$scope.updateEndDate = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					if ($scope.period.startDate > $scope.period.endDate) {
						$scope.period.endDate = angular.copy($scope.period.startDate);
						return;
					}
				}

				$scope.period.endDate = angular.copy($scope.period.endDate);
			};

			var forecastForOneWorkload = function (index) {
				var workload = $scope.workloads[index];
				if (!workload) {
					return;
				}
				workload.ShowProgress = true;

				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: [workload] })).
					success(function (data, status, headers, config) {
						if (data.Success) {
							workload.IsSuccess = true;
						} else {
							workload.IsFailed = true;
							workload.Message = data.Message;
						}
					}).
					error(function (data, status, headers, config) {
						workload.IsFailed = true;
						if (data)
							workload.Message = data.Message;
						else
							workload.Message = "Failed";
					})
					.finally(function () {
						forecastForOneWorkload(++index);
						workload.ShowProgress = false;
					});
			};

			$scope.nextStepAll = function (period) {
				if ($scope.disableNextStepAll()) {
					return;
				}
				$scope.modalLaunch = false;

				forecastForOneWorkload(0);
			};

			$scope.disableNextStepAll = function () {
				return $scope.moreThanOneYear() || $scope.isForecastRunning;
			};

			$scope.nextStepAdvanced = function (period) {
				$state.go('forecasting-target', { period: period });
			};

			$scope.disalbeNextStepAdvanced = function () {
				return $scope.moreThanOneYear();
			};

			$scope.setRangeClass = function (date, mode) {
				if (mode === 'day') {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var startDay = new Date($scope.period.startDate).setHours(12, 0, 0, 0);
					var endDay = new Date($scope.period.endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= startDay && dayToCheck <= endDay) {
						return 'in-date-range';
					}
				}
				return '';
			};
		}
	]);
