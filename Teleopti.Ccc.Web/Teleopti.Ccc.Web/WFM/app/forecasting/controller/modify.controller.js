(function() {
	'use strict';

	angular.module('wfm.forecasting').controller('ForecastModifyController', forecastModifyController);

	forecastModifyController.$inject = [
		'ForecastingService',
		'NoticeService',
		'$translate',
		'$state',
		'$scope',
		'skillIconService',
		'SkillTypeService'
	];

	function forecastModifyController(
		forecastingService,
		noticeSvc,
		$translate,
		$state,
		$scope,
		skillIconService,
		skillTypeService
	) {
		var vm = this;

		vm.loadChart = loadChart;
		vm.pointClick = pointClick;
		vm.selectedDayCount = [];
		vm.modifyPanelHelper = modifyPanelHelper;
		vm.campaignPanel = false;
		vm.overridePanel = false;
		vm.selectedScenario = null;
		vm.targetScenario = null;
		vm.forecastPeriod = {
			startDate: moment()
				.utc()
				.add(1, 'days')
				.toDate(),
			endDate: moment()
				.utc()
				.add(6, 'months')
				.toDate()
		};
		vm.savingToScenario = false;
		vm.getSkillIcon = skillIconService.get;

		vm.applyOverride = applyOverride;
		vm.applyCampaign = applyCampaign;
		vm.clearCampaign = clearCampaign;
		vm.clearOverride = clearOverride;
		vm.updateCampaignPreview = updateCampaignPreview;
		vm.getWorkloadForecastData = getWorkloadForecastData;
		vm.changeScenario = changeScenario;
		vm.forecastWorkload = forecastWorkload;
		vm.applyWipToScenario = applyWipToScenario;
		vm.exportToFile = exportToFile;
		vm.exportToScenario = exportToScenario;

		vm.isForecastRunning = false;
		vm.overrideStatus = {
			tasks: false,
			talkTime: false,
			acw: false
		};

		(function init() {
			manageLocalStorage();
			if (vm.noWorkloadFound) {
				return;
			}

			vm.dataName = skillTypeService.getSkillLabels(vm.selectedWorkload.SkillType);
			getScenarios();
		})();

		function resetForms() {
			vm.campaignPercentage = null;
		}

		function modifyPanelHelper(state) {
			if (vm.selectedDayCount === null || vm.selectedDayCount.length < 1) {
				vm.campaignPanel = false;
				vm.overridePanel = false;
				return;
			}
			if (state === true) {
				//overide
				vm.campaignPanel = false;
				vm.overridePanel = true;
			} else if (state === false) {
				//campaign
				vm.campaignPanel = true;
				vm.overridePanel = false;
			} else {
				vm.campaignPanel = false;
				vm.overridePanel = false;
			}
			resetForms();
		}

		function clearCampaign() {
			vm.campaignPercentage = 0;
			applyCampaign();
		}

		function updateCampaignPreview() {
			vm.sumOfCallsForSelectedDays = (
				(vm.selectedDayCount[0].value * (vm.campaignPercentage + 100)) /
				100
			).toFixed(1);
		}

		function applyCampaign() {
			vm.modifyMessage = null;
			vm.isForecastRunning = true;
			forecastingService.campaign(
				angular.toJson({
					ForecastDays: vm.selectedWorkload.Days,
					SelectedDays: vm.selectedDayCount,
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.selectedScenario.Id,
					CampaignTasksPercent: vm.campaignPercentage / 100
				}),
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.changesMade = true;
					vm.isForecastRunning = false;
				},
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.changesMade = true;
					vm.isForecastRunning = false;
				},
				function() {
					if (vm.modifyMessage) {
						noticeSvc.warning(vm.modifyMessage, 15000, true);
					} else {
						noticeSvc.success($translate.instant('AppliedACampaign'), 15000, true);
					}
					modifyPanelHelper();
					vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
				}
			);
		}

		function checkData(data) {
			return data != null;
		}

		function applyOverride(form) {
			vm.modifyMessage = null;
			vm.isForecastRunning = true;
			forecastingService.override(
				angular.toJson({
					SelectedDays: vm.selectedDayCount,
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.selectedScenario.Id,
					OverrideTasks: form.overrideTasksValue,
					OverrideAverageTaskTime: form.overrideTalkTimeValue,
					OverrideAverageAfterTaskTime: form.overrideAfterCallWorkValue,
					ShouldOverrideTasks: checkData(form.overrideTasksValue),
					ShouldOverrideAverageTaskTime: checkData(form.overrideTalkTimeValue),
					ShouldOverrideAverageAfterTaskTime: checkData(form.overrideAfterCallWorkValue),
					ForecastDays: vm.selectedWorkload.Days
				}),
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.changesMade = true;
					vm.isForecastRunning = false;
				},
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.changesMade = true;
					vm.isForecastRunning = false;
				},
				function() {
					if (vm.modifyMessage) {
						noticeSvc.warning(vm.modifyMessage, 15000, true);
					} else {
						noticeSvc.success($translate.instant('AppliedAOverride'), 15000, true);
					}

					modifyPanelHelper();
					vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
				}
			);
		}

		function clearOverride() {
			vm.isForecastRunning = true;
			forecastingService.override(
				angular.toJson({
					ForecastDays: vm.selectedWorkload.Days,
					SelectedDays: vm.selectedDayCount,
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.selectedScenario.Id,
					ShouldOverrideTasks: true,
					ShouldOverrideAverageTaskTime: true,
					ShouldOverrideAverageAfterTaskTime: true
				}),
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.isForecastRunning = false;
					vm.changesMade = true;
				},
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.modifyMessage = data.WarningMessage;
					vm.isForecastRunning = false;
					vm.changesMade = true;
				},
				function() {
					noticeSvc.success($translate.instant('ClearOverride'), 15000, true);

					modifyPanelHelper();
					vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
				}
			);
		}

		function loadChart() {
			return;
		}

		function pointClick(days) {
			vm.selectedDayCount = days;
		}

		function manageLocalStorage() {
			vm.noWorkloadFound = null;
			if (sessionStorage.currentForecastWorkload) {
				vm.selectedWorkload = angular.fromJson(sessionStorage.currentForecastWorkload);
			} else {
				vm.noWorkloadFound = true;
			}
		}

		function getScenarios() {
			vm.scenarios = [];
			forecastingService.scenarios.query().$promise.then(function(result) {
				result.forEach(function(s) {
					vm.scenarios.push(s);
				});
				changeScenario(vm.scenarios[0], false);
			});
		}

		function changeScenario(scenario, keepSelectedPeriod) {
			vm.selectedScenario = scenario;
			getWorkloadForecastData(keepSelectedPeriod);
		}

		function getWorkloadForecastData(keepSelectedPeriod) {
			vm.periodModal = false;

			vm.selectedWorkload.Days = [];
			vm.isForecastRunning = true;
			vm.scenarioNotForecasted = false;

			var wl = {
				ForecastStart: moment(vm.forecastPeriod.startDate).format(),
				ForecastEnd: moment(vm.forecastPeriod.endDate).format(),
				WorkloadId: vm.selectedWorkload.Workload.Id,
				ScenarioId: vm.selectedScenario.Id,
				HasUserSelectedPeriod: keepSelectedPeriod
			};

			forecastingService.result(
				wl,
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					if (!keepSelectedPeriod && vm.selectedWorkload.Days.length > 0) {
						vm.forecastPeriod = {
							startDate: moment(data.ForecastDays[0].Date)
								.utc()
								.toDate(),
							endDate: moment(data.ForecastDays[vm.selectedWorkload.Days.length - 1].Date)
								.utc()
								.toDate()
						};
					}
					vm.isForecastRunning = false;
					vm.scenarioNotForecasted = vm.selectedWorkload.Days.length === 0;
					vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
				},
				function(data, status, headers, config) {
					vm.selectedWorkload.Days = data.ForecastDays;
					vm.isForecastRunning = false;
					vm.scenarioNotForecasted = vm.selectedWorkload.Days.length === 0;
				}
			);
		}

		function forecastWorkload() {
			vm.forecastModal = false;
			vm.isForecastRunning = true;

			forecastingService.forecast(
				{
					ForecastStart: moment(vm.forecastPeriod.startDate).format(),
					ForecastEnd: moment(vm.forecastPeriod.endDate).format(),
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.selectedScenario.Id
				},
				function(data, status, headers, config) {
					vm.isForecastRunning = false;
					vm.closeConfirmationOnForecasting = false;
					if (data.WarningMessage && data.WarningMessage !== '') {
						noticeSvc.warning(data.WarningMessage, 15000, true);
					} else {
						vm.selectedWorkload.Days = data.ForecastDays;
						vm.scenarioNotForecasted = vm.selectedWorkload.Days.length === 0;
						vm.changesMade = true;
						vm.WarningMessage = '';
						vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
					}
				},
				function(data, status, headers, config) {
					vm.isForecastRunning = false;
					vm.forecastModal = false;
					vm.scenarioNotForecasted = vm.selectedWorkload.Days.length === 0;
					vm.changesMade = true;
				}
			);
		}

		function applyWipToScenario() {
			vm.savingToScenario = true;
			var tempForecastDays = [];
			for (var i = 0; i < vm.selectedWorkload.Days.length; i++) {
				if (vm.selectedWorkload.Days[i].IsInModification) {
					tempForecastDays.push(vm.selectedWorkload.Days[i]);
				}
			}

			forecastingService.applyToScenario(
				angular.toJson({
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.selectedScenario.Id,
					ForecastDays: tempForecastDays
				}),
				function(data, status, headers, config) {
					vm.savingToScenario = false;
					vm.changesMade = false;
					getWorkloadForecastData(true);
					noticeSvc.success(
						$translate.instant('SuccessfullyUpdatedPeopleCountColon') + ' ' + vm.selectedScenario.Name,
						15000,
						true
					);
				},
				function(data, status, headers, config) {
					vm.savingToScenario = false;
					vm.changesMade = false;
					getWorkloadForecastData(true);
				}
			);
		}

		function exportToScenario() {
			vm.scenarioExportModal = false;
			vm.savingToScenario = true;
			var tempForecastDays = vm.selectedWorkload.Days;
			forecastingService.applyToScenario(
				angular.toJson({
					WorkloadId: vm.selectedWorkload.Workload.Id,
					ScenarioId: vm.targetScenario.Id,
					ForecastDays: tempForecastDays
				}),
				function(data, status, headers, config) {
					vm.savingToScenario = false;
					noticeSvc.success(
						$translate.instant('SuccessfullyUpdatedPeopleCountColon') + ' ' + vm.targetScenario.Name,
						15000,
						true
					);
					vm.targetScenario = null;
				},
				function(data, status, headers, config) {
					vm.savingToScenario = false;
				}
			);
		}

		function exportToFile() {
			vm.exportModal = false;
			vm.isForecastRunning = true;
			forecastingService.exportForecast(
				{
					ForecastStart: moment(vm.forecastPeriod.startDate).format(),
					ForecastEnd: moment(vm.forecastPeriod.endDate).format(),
					ScenarioId: vm.selectedScenario.Id,
					SkillId: vm.selectedWorkload.SkillId,
					WorkloadId: vm.selectedWorkload.Workload.Id
				},
				function(data, status, headers, config) {
					var blob = new Blob([data], {
						type: headers['content-type']
					});
					var fileName =
						moment(vm.forecastPeriod.startDate).format('YYYY-MM-DD') +
						' - ' +
						moment(vm.forecastPeriod.endDate).format('YYYY-MM-DD') +
						'.xlsx';
					saveAs(blob, fileName);
					vm.isForecastRunning = false;
				},
				function(data, status, headers, config) {
					vm.isForecastRunning = false;
				}
			);
		}

		vm.exitConfigMode = function() {
			if (vm.stateName.length > 0) {
				$state.go(vm.stateName);
			} else {
				$state.go($state.params.returnState);
			}
		};

		$scope.$on('$stateChangeStart', function(event, next, current) {
			if (vm.changesMade) {
				event.preventDefault();
				vm.stateName = next.name;
				vm.closeConfirmation = true;
				vm.confirmDiscardDialog = false;
				vm.reforecastConfirmation = false;
				return;
			}

			if (vm.isForecastRunning) {
				event.preventDefault();
				vm.stateName = next.name;
				vm.closeConfirmationOnForecasting = true;
				return;
			}
		});
	}
})();
