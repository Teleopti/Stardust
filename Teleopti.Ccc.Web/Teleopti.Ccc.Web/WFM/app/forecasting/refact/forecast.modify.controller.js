(function() {
  'use strict';

  angular.module('wfm.forecasting').controller('ForecastModController', ForecastModCtrl);

  ForecastModCtrl.$inject = ['forecastingService', '$stateParams', '$window', 'NoticeService', '$translate'];

  function ForecastModCtrl(forecastingService, $stateParams, $window, NoticeService, $translate) {
    var vm = this;

    var storage = {};
    vm.loadChart = loadChart;
    vm.pointClick = pointClick;
    vm.selectedDayCount = [];
    vm.modifyPanelHelper = modifyPanelHelper;
    vm.campaignPanel = false;
    vm.overridePanel = false;
    vm.selectedScenario = null;
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

    vm.applyOverride = applyOverride;
    vm.applyCampaign = applyCampaign;
    vm.clearCampaign = clearCampaign;
    vm.clearOverride = clearOverride;
    vm.updateCampaignPreview = updateCampaignPreview;
    vm.getWorkloadForecastData = getWorkloadForecastData;
    vm.changeScenario = changeScenario;
    vm.forecastWorkload = forecastWorkload;
    vm.loadChart = loadChart;
    vm.applyWipToScenario = applyWipToScenario;
    vm.exportToFile = exportToFile;

    vm.isForecastRunning = false;
    vm.overrideStatus = {
      tasks: false,
      talkTime: false,
      acw: false
    };

    (function init() {
      manageLocalStorage();
      getScenarios();
    })();

    function resetForms() {
      vm.campaignPercentage = null;
    }

    function loadChart() {
      return;
    }

    function modifyPanelHelper(state) {
      if (vm.selectedDayCount === null || vm.selectedDayCount.length < 1) {
        vm.campaignPanel = false;
        vm.overridePanel = false;
        return;
      }
      if (state == true) {
        //overide
        vm.campaignPanel = false;
        vm.overridePanel = true;
      } else if (state == false) {
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
      vm.sumOfCallsForSelectedDays = (vm.selectedDayCount[0].value * (vm.campaignPercentage + 100) / 100).toFixed(
        1
      );
    }

    function applyCampaign() {
      forecastingService.campaign(
        angular.toJson({
          ForecastDays: vm.selectedWorkload.Days,
          SelectedDays: vm.selectedDayCount,
          WorkloadId: vm.selectedWorkload.Workload.Id,
          ScenarioId: vm.selectedScenario.Id,
          CampaignTasksPercent: vm.campaignPercentage / 100
        }),
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function() {
          NoticeService.success($translate.instant('CampaignValuesUpdated'), 5000, true);
          modifyPanelHelper();
          vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
        }
      );
    }

    function checkData(data) {
      if (data == null) {
        return false;
      } else {
        return true;
      }
    }

    function applyOverride(form) {
      forecastingService.override(
        angular.toJson({
          SelectedDays: vm.selectedDayCount,
          WorkloadId: vm.selectedWorkload.Workload.Id,
          ScenarioId: vm.selectedScenario.Id,
          OverrideTasks: form.overrideTasksValue,
          OverrideTalkTime: form.overrideTalkTimeValue,
          OverrideAfterCallWork: form.overrideAfterCallWorkValue,
          ShouldOverrideTasks: checkData(form.overrideTasksValue),
          ShouldOverrideTalkTime: checkData(form.overrideTalkTimeValue),
          ShouldOverrideAfterCallWork: checkData(form.overrideAfterCallWorkValue),
          ForecastDays: vm.selectedWorkload.Days
        }),
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function() {
          NoticeService.success($translate.instant('OverrideValuesUpdated'), 5000, true);
          modifyPanelHelper();
          vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
        }
      );
    }

    function clearOverride() {
      forecastingService.override(
        angular.toJson({
          ForecastDays: vm.selectedWorkload.Days,
          SelectedDays: vm.selectedDayCount,
          WorkloadId: vm.selectedWorkload.Workload.Id,
          ScenarioId: vm.selectedScenario.Id,
          ShouldSetOverrideTasks: true,
          ShouldSetOverrideTalkTime: true,
          ShouldSetOverrideAfterCallWork: true
        }),
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data;
          vm.changesMade = true;
        },
        function() {
          NoticeService.success($translate.instant('OverrideValuesCleared'), 5000, true);
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

    function UniqueArraybyId(collection, keyname) {
      var output = [],
      keys = [];

      angular.forEach(collection, function(item) {
        var key = item[keyname];
        if (keys.indexOf(key) === -1) {
          keys.push(key);
          output.push(item);
        }
      });
      return output;
    }

    function manageLocalStorage() {
      vm.noWorkloadFound = null;
      if (sessionStorage.currentForecastWorkload) {
        vm.selectedWorkload = JSON.parse(sessionStorage.currentForecastWorkload);
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
        changeScenario(vm.scenarios[0]);
      });
    }

    function changeScenario(scenario) {
      vm.selectedScenario = scenario;
      getWorkloadForecastData();
    }

    function getWorkloadForecastData() {
      vm.selectedWorkload.Days = [];
      vm.isForecastRunning = true;
      vm.scenarioNotForecasted = false;

      var resultStartDate = moment().utc();
      var resultEndDate = moment(resultStartDate).add(6, 'months');

      var wl = {
        ForecastStart: vm.forecastPeriod.startDate,
        ForecastEnd: vm.forecastPeriod.endDate,
        WorkloadId: vm.selectedWorkload.Workload.Id,
        ScenarioId: vm.selectedScenario.Id
      };

      forecastingService.result(
        wl,
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data.ForecastDays;
          vm.isForecastRunning = false;
          vm.periodModal = false;
          if (vm.selectedWorkload.Days.length === 0) {
            vm.scenarioNotForecasted = true;
          }
        },
        function(data, status, headers, config) {
          vm.selectedWorkload.Days = data.ForecastDays;
          vm.isForecastRunning = false;
          vm.periodModal = false;
        }
      );
    }

    function forecastWorkload() {
      vm.isForecastRunning = true;
      var temp = {
        WorkloadId: vm.selectedWorkload.Workload.Id,
        ForecastMethodId: -1
      };

      forecastingService.forecast(
        angular.toJson({
          ForecastStart: vm.forecastPeriod.startDate,
          ForecastEnd: vm.forecastPeriod.endDate,
          Workload: temp,
          ScenarioId: vm.selectedScenario.Id,
          BlockToken: vm.blockToken,
          IsLastWorkload: true
        }),
        function(data, status, headers, config) {
          vm.isForecastRunning = false;
          vm.forecastModal = false;
          vm.selectedWorkload.Days = data.ForecastDays;
        },
        function(data, status, headers, config) {
          vm.isForecastRunning = false;
          vm.forecastModal = false;
        }
      );
    }

    function applyWipToScenario() {
      vm.savingToScenario = true;
      var tempForecastDays = vm.selectedWorkload.Days;

      forecastingService.applyToScenario(
        angular.toJson({
          WorkloadId: vm.selectedWorkload.Workload.Id,
          ScenarioId: vm.selectedScenario.Id,
          ForecastDays: tempForecastDays
        }),
        function(data, status, headers, config) {
          vm.savingToScenario = false;
          vm.changesMade = false;
          getWorkloadForecastData();
          NoticeService.success(vm.selectedScenario.Name + ' ' + 'scenario was updated', 5000, true);
        },
        function(data, status, headers, config) {
          vm.savingToScenario = false;
          vm.changesMade = false;
          getWorkloadForecastData();
        }
      );
    }

    function exportToFile() {
      vm.isForecastRunning = true;
      forecastingService.exportForecast(
        angular.toJson({
          ForecastStart: vm.forecastPeriod.startDate,
          ForecastEnd: vm.forecastPeriod.endDate,
          ScenarioId: vm.selectedScenario.Id,
          SkillId: vm.selectedWorkload.SkillId,
          WorkloadId: vm.selectedWorkload.Workload.Id
        }),
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
          vm.exportModal = false;
        },
        function(data, status, headers, config) {
          vm.isForecastRunning = false;
        }
      );
    }
  }
})();
