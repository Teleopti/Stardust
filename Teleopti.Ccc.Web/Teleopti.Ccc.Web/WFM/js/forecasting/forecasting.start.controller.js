'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingStartCtrl', [
		'$scope', '$state', 'Forecasting', '$http', '$filter',
		function ($scope, $state, forecasting, $http, $filter) {
			$scope.isForecastRunning = false;
			forecasting.status.get().$promise.then(function(result) {
				$scope.isForecastRunning = result.IsRunning;
			});
			var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
			var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate };

			$scope.workloads = [];
			forecasting.skills.query().$promise.then(function(result) {
				$scope.skills = result;
				angular.forEach($scope.skills, function(skill) {
					angular.forEach(skill.Workloads, function(workload) {
						$scope.workloads.push({ Id: workload.Id, Name: skill.Name + " - " + workload.Name, ChartId: "chart" + workload.Id });
					});
				});

				$filter('orderBy')($scope.workloads, 'Name');
			});

			$scope.modalInfo = {
				forecastForAll: false,
				forecastForOneWorkload: false
			};
			$scope.modalLaunch = false;
			$scope.displayModal = function (workload) {
				if (workload) {
					$scope.modalInfo.forecastForAll = false;
					$scope.modalInfo.forecastForOneWorkload = true;
					$scope.modalInfo.selectedWorkload = workload;
				} else {
					$scope.modalInfo.forecastForAll = true;
					$scope.modalInfo.forecastForOneWorkload = false;
				}
				$scope.modalLaunch = true;
			};
			$scope.cancelModal = function () {
				$scope.modalLaunch = false;
			};

			$scope.getForecastResult = function (workload) {
				workload.forecastResultLoaded = false;

				$scope.resultChartData = [];
				var resultStartDate = moment().utc().add(1, 'days');
				var resultEndDate = moment(resultStartDate).add(6, 'months');
				$http.post("../api/Forecasting/ForecastResult", JSON.stringify({ ForecastStart: resultStartDate.toDate(), ForecastEnd: resultEndDate.toDate(), WorkloadId: workload.Id })).
					success(function (data, status, headers, config) {
						angular.forEach(data.Days, function (day) {
							day.date = new Date(Date.parse(day.date));
						});
						workload.resultChartData = data.Days;
						workload.forecastResultLoaded = true;
						var chart = c3.generate({
							bindto: "#" + workload.ChartId,
							data: {
								json:
									workload.resultChartData
								,
								keys: {
									// x: 'name', // it's possible to specify 'x' when category axis
									x: 'date',
									value: ['vc', 'vaht', 'vacw']
								},
								axes: {
									vaht: 'y2',
									vacw: 'y2'
								},
								selection: {
									enabled: true,
									grouped: true,
									isselectable: function (chartPoint) {
										if (chartPoint.id === 'vacw' || chartPoint.id === 'vaht')
											return false;
										return true;
									}
								},
								names: {
									vc: 'Calls <',
									vaht: 'Talk time >',
									vacw: 'ACW >',
								}
							},
							axis: {
								x: {
									type: 'timeseries',
									tick: {
										format: '%Y-%m-%d'
									}
								},
								y2: {
									show: true
								}
							},
							subchart: {
								show: true
							}
						});
						console.log(chart);

					}).
					error(function (data, status, headers, config) {
						$scope.error = { message: "Failed to get forecast result." };
						workload.forecastResultLoaded = true;
					});
			};

			$scope.chartInfo = {
				resultChartDataColumns: [
					{ id: "vc", type: "line", name: "Calls" },
					{ id: "vaht", type: "line", name: "Talk time" },
					{ id: "vacw", type: "line", name: "ACW" }
				],
				dataX: {
					 id: "date"
				}
			};

			var forecastWorkload = function (workload, finallyCallback) {
				workload.ShowProgress = true;
				workload.IsSuccess = false;
				workload.IsFailed = false;
				var workloadToSend = { WorkloadId: workload.Id };
				if (workload.selectedMethod) {
					workloadToSend.ForecastMethodId = workload.selectedMethod;
				} else {
					workloadToSend.ForecastMethodId = -1;
				}
				$http.post('../api/Forecasting/Forecast', JSON.stringify({ ForecastStart: $scope.period.startDate, ForecastEnd: $scope.period.endDate, Workloads: [workloadToSend] })).
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
						workload.ShowProgress = false;
						if (workload.forecastResultLoaded) {
							$scope.getForecastResult(workload);
						}
						finallyCallback();
					});
			}

			var forecastAllStartFromIndex = function (index) {
				var workload = $scope.workloads[index];
				if (!workload) {
					$scope.isForecastRunning = false;
					return;
				}
				forecastWorkload(workload, function () {
					forecastAllStartFromIndex(++index);
				});
			};

			$scope.doForecast = function () {
				if ($scope.disableDoForecast()) {
					return;
				}
				$scope.modalLaunch = false;
				$scope.isForecastRunning = true;
				if ($scope.modalInfo.forecastForOneWorkload) {
					forecastWorkload($scope.modalInfo.selectedWorkload, function () {
						$scope.isForecastRunning = false;
					});
				} else {
					forecastAllStartFromIndex(0);
				}
			};

			$scope.disableDoForecast = function () {
				return $scope.moreThanOneYear() || $scope.isForecastRunning;
			};

			$scope.nextStepAdvanced = function (workload) {
				$state.go('forecasting-advanced', { workloadId: workload.Id, workloadName: workload.Name });
			};

			$scope.moreThanOneYear = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					var dateDiff = new Date($scope.period.endDate - $scope.period.startDate);
					dateDiff.setDate(dateDiff.getDate() - 1);
					return dateDiff.getFullYear() - 1970 >= 1;
				} else
					return false;
			};

			$scope.setRangeClass = function (date, mode) {
				if (mode === 'day') {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var startDay = new Date($scope.period.startDate).setHours(12, 0, 0, 0);
					var endDay = new Date($scope.period.endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= startDay && dayToCheck <= endDay) {
						return 'range';
					}
				}
				return '';
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
		}
	]);
