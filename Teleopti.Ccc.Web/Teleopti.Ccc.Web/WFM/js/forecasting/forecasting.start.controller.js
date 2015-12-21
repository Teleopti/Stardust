(function () {
	'use strict';

	angular.module('wfm.forecasting')
		.controller('ForecastingStartCtrl', [
			'$scope', '$state', 'Forecasting', '$http', '$filter', '$interval', 'Toggle', '$stateParams', '$translate',
			function ($scope, $state, forecasting, $http, $filter, $interval, toggleService, $stateParams, $translate) {
				$scope.isForecastRunning = false;
				function updateRunningStatus() {
					forecasting.status.get().$promise.then(function (result) {
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

				$scope.createSkillToggle = false;
				$scope.isCreateSkillEnabled = false;

				toggleService.isFeatureEnabled.query({ toggle: 'WfmForecast_CreateSkill_34591' }).$promise.then(function (result) {
					$scope.createSkillToggle = result.IsEnabled;
				});

				var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
				var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
				$scope.period = { startDate: startDate, endDate: endDate };

				$scope.workloads = [];

				var getName = function(skillName, workloadName) {
					var name;
					if (!workloadName || workloadName == skillName)
						name = skillName;
					else {
						name = skillName + " - " + workloadName;
					}
					return name;
				};

				var getSkills = function () {
					var skillsPromise = forecasting.skills.query().$promise;
					skillsPromise.then(function (result) {
						$scope.isCreateSkillEnabled = $scope.createSkillToggle && result.IsPermittedToModifySkill;
						var skills = result.Skills;
						var workloads = [];
						var addedWorkload = null;
						angular.forEach(skills, function (skill) {
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
				$scope.changeScenario = function (workload) {
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

				$scope.modalModifyInfo = {};

				var calculateCampaignCalls = function () {
					return ($scope.sumOfCallsForSelectedDays * ($scope.modalModifyInfo.campaignPercentage + 100) / 100).toFixed(1);
				};

				$scope.modifyDays = [];
				$scope.sumOfCallsForSelectedDays = 0;
				$scope.campaignPercentageConst = {
					max: 1000,
					min: -100
				};

				var handleModifyDays = function (workload) {
					var tempsum = 0;
					$scope.modifyDays = [];
					var isOnlyOneDaySelected = workload.selectedDays().length/3 == 1;
					angular.forEach(workload.selectedDays(), function (value) {
						if (value.id == 'vtc') {
							if (isOnlyOneDaySelected) {
								$scope.modalModifyInfo.overrideTasks = Math.round(value.value * 10) / 10;
							}
							$scope.modifyDays.push({
								date: new Date(Date.UTC(value.x.getFullYear(), value.x.getMonth(), value.x.getDate(), 0, 0, 0))
							});
							tempsum += value.value;
						}
						if (isOnlyOneDaySelected) {
							if (value.id == 'vttt') {
								$scope.modalModifyInfo.overrideTalkTime = Math.round(value.value * 10) / 10;
							}
							if (value.id == 'vtacw') {
								$scope.modalModifyInfo.overrideAcw = Math.round(value.value * 10) / 10;
							}
						}
					});
					$scope.sumOfCallsForSelectedDays = tempsum.toFixed(1);
				};

				$scope.modalModifyLaunch = false;
				$scope.displayModifyModal = function (workload) {
					if ($scope.disableModify(workload)) {
						return;
					}
					$scope.modalModifyLaunch = true;
					$scope.modalModifyInfo.overrideTasks = undefined;
					$scope.modalModifyInfo.overrideTalkTime = undefined;
					$scope.modalModifyInfo.overrideAcw = undefined;
					handleModifyDays(workload);
					$scope.modalModifyInfo.campaignPercentage = 0;
					$scope.modalModifyInfo.shouldSetOverrideTasks = false;
					$scope.modalModifyInfo.shouldSetOverrideTalkTime = false;
					$scope.modalModifyInfo.shouldSetOverrideAfterCallWork = false;
					$scope.modalModifyInfo.selectedWorkload = workload;
					$scope.modalModifyInfo.selectedScenario = workload.Scenario;
					$scope.modalModifyInfo.selectedDaysText = buildSelectedDaysText();
					$scope.modalModifyInfo.selectedDayCount = $scope.modifyDays.length;
					$scope.modalModifyInfo.selectedDaySpan = selectedDaySpanString();
 					$scope.sumOfCallsForSelectedDaysWithCampaign = calculateCampaignCalls();
				};

				var buildSelectedDaysText = function() {
					if ($scope.modifyDays.length == 1) {
						var singleDate = $filter('date')($scope.modifyDays[0].date, 'shortDate');
						return $translate.instant('SelectedDayIs').replace('{0}', singleDate);
					}
					var firstDate = $filter('date')($scope.modifyDays[0].date, 'shortDate');
					var lastDate = $filter('date')($scope.modifyDays[$scope.modifyDays.length - 1].date, 'shortDate');
					var text = $translate.instant('DaysAreSelectedFrom');
					text = text.replace('{0}', $scope.modifyDays.length);
					text = text.replace('{1}', firstDate);
					text = text.replace('{2}', lastDate);
					return text;
				}

				var selectedDaySpanString = function () {
					if ($scope.modifyDays.length == 1)
						return $filter('date')($scope.modifyDays[0].date, 'shortDate');
					else {
						return $filter('date')($scope.modifyDays[0].date, 'shortDate') + ' - ' + $filter('date')($scope.modifyDays[$scope.modifyDays.length - 1].date, 'shortDate');
					}
				}

				$scope.campaignPercentageChanged = function (campaignForm) {
					if (campaignForm) {
						if (campaignForm.campaignPercentageInput.$valid) {
							// do nothing
						} else if (campaignForm.campaignPercentageInput.$error.max) {
							$scope.modalModifyInfo.campaignPercentage = $scope.campaignPercentageConst.max;
						} else if (campaignForm.campaignPercentageInput.$error.min) {
							$scope.modalModifyInfo.campaignPercentage = $scope.campaignPercentageConst.min;
						}
						$scope.sumOfCallsForSelectedDaysWithCampaign = calculateCampaignCalls();
					}
				};

				$scope.cancelModifyModal = function () {
					$scope.modalModifyLaunch = false;
				};

				$scope.formatSelectedDayCount = function(count) {
					return $scope.formatDayCount(count / 3, true);
				};

				$scope.formatDayCount = function (count, withParenthesis) {
					if (count > 0)
						if (withParenthesis)
							return '(' + count + ')';
						else
							return count;
					else
						return '';
				};

				$scope.getForecastResult = function (workload) {
					workload.forecastResultLoaded = false;

					var resultStartDate = moment().utc().add(1, 'days');
					var resultEndDate = moment(resultStartDate).add(6, 'months');
					$http.post("../api/Forecasting/ForecastResult", JSON.stringify(
						{
							ForecastStart: resultStartDate.toDate(),
							ForecastEnd: resultEndDate.toDate(),
							WorkloadId: workload.Id,
							ScenarioId: workload.Scenario.Id
						})).
						success(function (data, status, headers, config) {

							for (var i = 0; i < data.Days.length; i++) {
								var day = data.Days[i];
								day.date = new Date(Date.parse(day.date));
							}
							workload.Refresh(data.Days);
							workload.forecastResultLoaded = true;
						}).
						error(function (data, status, headers, config) {
							$scope.error = { message: "Failed to get forecast result." };
							workload.forecastResultLoaded = true;
						});
				};


				$scope.disableModify = function (workload) {
					if ($scope.isForecastRunning) {
						return true;
					}
					return workload.selectedDays().length == 0;
				};

				$scope.applyCampaign = function () {
					if ($scope.disableApplyCampaign()) {
						return;
					}
					$scope.modalModifyLaunch = false;
					$scope.isForecastRunning = true;
					var workload = $scope.modalModifyInfo.selectedWorkload;
					workload.ShowProgress = true;
					workload.IsSuccess = false;
					workload.IsFailed = false;
					$http.post("../api/Forecasting/Campaign", JSON.stringify(
						{
							Days: $scope.modifyDays,
							WorkloadId: workload.Id,
							ScenarioId: workload.Scenario.Id,
							CampaignTasksPercent: $scope.modalModifyInfo.campaignPercentage
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

				$scope.applyOverride = function (isFormValid) {
					if (!isFormValid || $scope.disableApplyOverride()) {
						return;
					}
					$scope.modalModifyLaunch = false;
					$scope.isForecastRunning = true;
					$scope.overrideTasks = false;
					var workload = $scope.modalModifyInfo.selectedWorkload;
					workload.ShowProgress = true;
					workload.IsSuccess = false;
					workload.IsFailed = false;
					$http.post("../api/Forecasting/Override", JSON.stringify(
						{
							Days: $scope.modifyDays,
							WorkloadId: workload.Id,
							ScenarioId: workload.Scenario.Id,
							OverrideTasks: $scope.modalModifyInfo.overrideTasks,
							OverrideTalkTime: $scope.modalModifyInfo.overrideTalkTime,
							OverrideAfterCallWork: $scope.modalModifyInfo.overrideAcw,
							ShouldSetOverrideTasks: $scope.modalModifyInfo.shouldSetOverrideTasks,
							ShouldSetOverrideTalkTime: $scope.modalModifyInfo.shouldSetOverrideTalkTime,
							ShouldSetOverrideAfterCallWork: $scope.modalModifyInfo.shouldSetOverrideAfterCallWork
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

				$scope.clearOverride = function () {
					if ($scope.disableClearOverride()) {
						return;
					}
					$scope.modalModifyLaunch = false;
					$scope.isForecastRunning = true;
					var workload = $scope.modalModifyInfo.selectedWorkload;
					workload.ShowProgress = true;
					workload.IsSuccess = false;
					workload.IsFailed = false;
					$http.post("../api/Forecasting/Override", JSON.stringify(
						{
							Days: $scope.modifyDays,
							WorkloadId: workload.Id,
							ScenarioId: workload.Scenario.Id,
							ShouldSetOverrideTasks: true,
							ShouldSetOverrideTalkTime: true,
							ShouldSetOverrideAfterCallWork: true
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

				if (c3.applyFixForForecast) c3.applyFixForForecast(function () {
					$scope.$apply();
				});

				$scope.$on('$destroy', function () {
					if (c3.restoreFixForForecast)
						c3.restoreFixForForecast();

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
						success(function (data, status, headers, config) {
							if (data.Success) {
								workload.IsSuccess = true;
							} else {
								workload.IsFailed = true;
								workload.Message = data.Message;
							}
							blockToken = data.BlockToken;
						})
						.error(function (data, status, headers, config) {
							workload.IsFailed = true;
							if (data)
								workload.Message = data.Message;
							else
								workload.Message = "Failed";
							blockToken = data.BlockToken;
						})
						.finally(function () {
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

				$scope.disableApplyOverride = function () {
					return $scope.isForecastRunning
						||
						(!$scope.modalModifyInfo.shouldSetOverrideTasks
							&& !$scope.modalModifyInfo.shouldSetOverrideTalkTime
							&& !$scope.modalModifyInfo.shouldSetOverrideAfterCallWork);
				};

				$scope.disableClearOverride = function () {
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

				$scope.createSkill = function () {
					$state.go('forecasting.skillcreate');
				};
			}
		]);
})();
