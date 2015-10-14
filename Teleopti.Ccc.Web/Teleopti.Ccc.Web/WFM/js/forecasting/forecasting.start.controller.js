﻿'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingStartCtrl', [
		'$scope', '$state', 'Forecasting', '$http', '$filter', '$interval', 'Toggle', '$stateParams',
		function ($scope, $state, forecasting, $http, $filter, $interval, toggleService, $stateParams) {
			$scope.isForecastRunning = false;
			function updateRunningStatus() {
				forecasting.status.get().$promise.then(function(result) {
					$scope.isForecastRunning = result.IsRunning;
				});
			};

			var stopPolling;
			function startPoll() {
				updateRunningStatus();
				stopPolling = $interval(updateRunningStatus, 10 * 1000);
			}
			startPoll();

			function cancelPoll() {
				$scope.isForecastRunning = false;
				if (angular.isDefined(stopPolling)) {
					$interval.cancel(stopPolling);
				}
			}

			$scope.isCreateSkillEnabled = false;
			toggleService.isFeatureEnabled.query({ toggle: 'WfmForecast_CreateSkill_34591' }).$promise.then(function (result) {
				$scope.isCreateSkillEnabled = result.IsEnabled;
			});

			var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
			var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate };

			$scope.workloads = [];

			var getName = function (skillName, workloadName) {
				var name;
				if (!workloadName || workloadName == skillName)
					name = skillName;
				else {
					name = skillName + " - " + workloadName;
				}
				return name;
			}

			var getSkills = function() {
				var skillsPromise = forecasting.skills.query().$promise;
				skillsPromise.then(function(result) {
					var skills = result;
					var workloads = [];
					var addedWorkload = null;
					angular.forEach(skills, function(skill) {
						angular.forEach(skill.Workloads, function (workload) {
							var w = {
								Id: workload.Id,
								Name: getName(skill.Name, workload.Name),
								ChartId: "chart" + workload.Id,
								Scenario: $scope.modalForecastingInfo.selectedScenario
							};
							if ($stateParams.workloadId == workload.Id) {
								w.preselected = true;
								$scope.getForecastResult(w);
								addedWorkload = w;
							} else {
								workloads.push(w);
							}
						});
					});
					workloads = $filter('orderBy')(workloads, 'Name');
					if (addedWorkload) {
						workloads.unshift(addedWorkload);
					}
					$scope.workloads = workloads;
				});
			};

			$scope.scenarios = [];
			var scenariosPromise = forecasting.scenarioList.$promise;
			scenariosPromise.then(function (result) {
				$scope.scenarios = result;
				$scope.modalForecastingInfo.selectedScenario = result[0];
				getSkills();
			});
			$scope.changeScenario = function(workload) {
				$scope.getForecastResult(workload);
			};

			$scope.modalForecastingInfo = {
				forecastForAll: false,
				forecastForOneWorkload: false
			};
			$scope.modalForecastingLaunch = false;
			$scope.displayForecastingModal = function (workload) {
				if ($scope.isForecastRunning) {
					return;
				}
				if (workload) {
					$scope.modalForecastingInfo.forecastForAll = false;
					$scope.modalForecastingInfo.forecastForOneWorkload = true;
					$scope.modalForecastingInfo.selectedWorkload = workload;
					$scope.modalForecastingInfo.selectedScenario = workload.Scenario;
				} else {
					$scope.modalForecastingInfo.forecastForAll = true;
					$scope.modalForecastingInfo.forecastForOneWorkload = false;
				}
				$scope.modalForecastingLaunch = true;
			};
			$scope.cancelForecastingModal = function () {
				$scope.modalForecastingLaunch = false;
			};

			$scope.modalCampaignInfo = { };

			var calculateCampaignCalls = function() {
				return ($scope.sumOfCallsForSelectedDays * ($scope.modalCampaignInfo.campaignPercentage + 100) / 100).toFixed(1);
			};

			$scope.modalCampaignLaunch = false;
			$scope.displayCampaignModal = function (workload) {
				if ($scope.disableAddCampaign()) {
					return;
				}
				$scope.modalCampaignLaunch = true;
				$scope.getCampaignDays();
				$scope.modalCampaignInfo.campaignPercentage = 0;
				$scope.modalCampaignInfo.selectedWorkload = workload;
				$scope.modalCampaignInfo.selectedScenario = workload.Scenario;
				$scope.sumOfCallsForSelectedDaysWithCampaign = calculateCampaignCalls();
			};
			
			var campaignDays = [];
			$scope.sumOfCallsForSelectedDays = 0;

			$scope.campaignPercentageConst = {
				max: 1000,
				min: -100
			};

			$scope.campaignPercentageChanged = function (campaignForm) {
				if (campaignForm) {
					if (campaignForm.campaignPercentageInput.$valid) {
						// do nothing
					} else if (campaignForm.campaignPercentageInput.$error.max) {
						$scope.modalCampaignInfo.campaignPercentage = $scope.campaignPercentageConst.max;
					} else if (campaignForm.campaignPercentageInput.$error.min) {
						$scope.modalCampaignInfo.campaignPercentage = $scope.campaignPercentageConst.min;
					}
					$scope.sumOfCallsForSelectedDaysWithCampaign = calculateCampaignCalls();
				}
			};

			$scope.getCampaignDays = function () {
				var tempsum = 0;
				campaignDays = [];
				angular.forEach($scope.chart.selected(), function (value) {
					campaignDays.push({
						date: value.x
					});
					tempsum += value.value;
				});
				$scope.sumOfCallsForSelectedDays = tempsum.toFixed(1);
			};
		
			$scope.cancelCampaignModal = function () {
				$scope.modalCampaignLaunch = false;
			};

			$scope.selectedDayCount = function () {
				if ($scope.chart && $scope.chart.selected())
					return $scope.chart.selected().length;
				return '';
			};

			$scope.selectedDayCountParenthesis = function () {
				if ($scope.selectedDayCount() > 0)
					return '(' + $scope.chart.selected().length + ')';
				return '';
			};
 
			$scope.chart = undefined;

			$scope.getForecastResult = function (workload) {
				workload.forecastResultLoaded = false;

				$scope.resultChartData = [];
				var resultStartDate = moment().utc().add(1, 'days');
				var resultEndDate = moment(resultStartDate).add(6, 'months');
				$http.post("../api/Forecasting/ForecastResult", JSON.stringify({ ForecastStart: resultStartDate.toDate(), ForecastEnd: resultEndDate.toDate(), WorkloadId: workload.Id, ScenarioId: workload.Scenario.Id })).
					success(function(data, status, headers, config) {
						angular.forEach(data.Days, function(day) {
							day.date = new Date(Date.parse(day.date));
						});
						workload.resultChartData = data.Days;
						workload.forecastResultLoaded = true;
						$scope.chart = c3.generate({
							bindto: "#" + workload.ChartId,
							data: {
								json:
									workload.resultChartData,
								keys: {
									// x: 'name', // it's possible to specify 'x' when category axis
									x: 'date',
									value: ['vtc', 'vc', 'vaht', 'vacw']
								},
								axes: {
									vaht: 'y2',
									vacw: 'y2'
								},
								selection: {
									enabled: true,
									grouped: true,
									draggable: true,
									isselectable: function(chartPoint) {
										if (chartPoint.id === 'vacw' || chartPoint.id === 'vtc' || chartPoint.id === 'vaht')
											return false;
										return true;
									}
								},
								names: {
									vtc: 'Total Calls <',
									vc: 'Calls <',
									vaht: 'Talk time >',
									vacw: 'ACW >'
								},
								colors: {
									vtc: '#0099FF',
									vc: '#99D6FF',
									vaht: '#9CCC65',
									vacw: '#F488C8'
								},
								onclick: function() {
									$scope.$digest();
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
							},
							tooltip: {
								format: {
									value: d3.format('.1f')
								}
							}
						});
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

			$scope.disableAddCampaign = function () {
				if ($scope.isForecastRunning) {
					return true;
				}
				if ($scope.chart && $scope.chart.selected())
					return $scope.chart.selected().length == 0;
				return true;
			};

			$scope.clearChartSelection = function() {
				$scope.chart.unzoom();
				$scope.chart.unselect(['vc']);
			};

			$scope.applyCampaign = function () {
				if ($scope.disableApplyCampaign()) {
					return;
				}
				$scope.modalCampaignLaunch = false;
				$scope.isForecastRunning = true;
				var workload = $scope.modalCampaignInfo.selectedWorkload;
				workload.ShowProgress = true;
				workload.IsSuccess = false;
				workload.IsFailed = false;
				$http.post("../api/Forecasting/AddCampaign", JSON.stringify(
					{
						Days: campaignDays,
						WorkloadId: workload.Id,
						ScenarioId: workload.Scenario.Id,
						CampaignTasksPercent: $scope.modalCampaignInfo.campaignPercentage
					}))
					.success(function (data, status, headers, config) {
						if (data.Success) {
							workload.IsSuccess = true;
						} else {
							workload.IsFailed = true;
							workload.Message = data.Message;
						}
					})
					.error(function (data, status, headers, config) {
						workload.IsFailed = true;
						if (data)
							workload.Message = data.Message;
						else
							workload.Message = "Failed";
					})
					.finally(function () {
						workload.ShowProgress = false;
						$scope.isForecastRunning = false;
						if (workload.forecastResultLoaded) {
							$scope.getForecastResult(workload);
						}
					});
			};

			if (c3.applyFixForForecast) c3.applyFixForForecast(function() {
				$scope.$digest();
			});
			$scope.$on('$destroy', function () {
				if (c3.restoreFixForForecast) c3.restoreFixForForecast();

				cancelPoll();
			});

			var forecastWorkload = function (workload, finallyCallback, blockToken, isLastWorkload) {
				workload.ShowProgress = true;
				workload.IsSuccess = false;
				workload.IsFailed = false;
				var workloadToSend = { WorkloadId: workload.Id };
				if (workload.selectedMethod) {
					workloadToSend.ForecastMethodId = workload.selectedMethod;
				} else {
					workloadToSend.ForecastMethodId = -1;
				}
				$http.post('../api/Forecasting/Forecast', JSON.stringify(
					{
						ForecastStart: $scope.period.startDate,
						ForecastEnd: $scope.period.endDate,
						Workloads: [workloadToSend],
						ScenarioId: $scope.modalForecastingInfo.selectedScenario.Id,
						BlockToken: blockToken,
						IsLastWorkload: isLastWorkload
					})).
					success(function(data, status, headers, config) {
						if (data.Success) {
							workload.IsSuccess = true;
						} else {
							workload.IsFailed = true;
							workload.Message = data.Message;
						}
						blockToken = data.BlockToken;
					})
					.error(function(data, status, headers, config) {
						workload.IsFailed = true;
						if (data)
							workload.Message = data.Message;
						else
							workload.Message = "Failed";
						blockToken = data.BlockToken;
					})
					.finally(function() {
						workload.ShowProgress = false;
						workload.Scenario = $scope.modalForecastingInfo.selectedScenario;
						if (workload.forecastResultLoaded) {
							$scope.getForecastResult(workload);
						}
						finallyCallback(blockToken);
					});
			}

			var forecastAllStartFromIndex = function (index, blockToken) {
				var workload = $scope.workloads[index];
				if (!workload) {
					$scope.isForecastRunning = false;
					return;
				}
				var isLastWorkload = $scope.workloads.length === (index + 1);
				forecastWorkload(workload, function (token) {
					var newIndex = ++index;
					forecastAllStartFromIndex(newIndex, token);
				}, blockToken, isLastWorkload);
			};

			$scope.doForecast = function () {
				if ($scope.disableDoForecast()) {
					return;
				}
				$scope.modalForecastingLaunch = false;
				$scope.isForecastRunning = true;
				if ($scope.modalForecastingInfo.forecastForOneWorkload) {
					forecastWorkload($scope.modalForecastingInfo.selectedWorkload, function () {
						$scope.isForecastRunning = false;
					}, null, true);
				} else {
					forecastAllStartFromIndex(0);
				}
			};

			$scope.disableDoForecast = function () {
				return $scope.moreThanOneYear() || $scope.isForecastRunning;
			};

			$scope.disableApplyCampaign = function () {
				return $scope.isForecastRunning;
			};

			$scope.nextStepAdvanced = function (workload) {
				$state.go('forecasting.advanced', { workloadId: workload.Id, workloadName: workload.Name });
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

			$scope.createSkill = function() {
				$state.go('forecasting.skillcreate');
			};
		}
	]);
