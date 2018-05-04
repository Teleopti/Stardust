(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastModController', ForecastModCtrl);

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
      startDate: moment().utc().add(1, 'days').toDate(),
      endDate: moment().utc().add(6, 'months').toDate()
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
      if (vm.selectedDayCount === null || vm.selectedDayCount.length<1) {
        return;
      }
      if (state == true) {
        //overide
        vm.campaignPanel = false;
        vm.overridePanel = true;
      }
      else if (state == false){
        //campaign
        vm.campaignPanel = true;
        vm.overridePanel = false;
      }
      else{
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
      vm.sumOfCallsForSelectedDays = (vm.selectedDayCount[0].value * (vm.campaignPercentage + 100) / 100).toFixed(1);
    }

    function applyCampaign() {
      forecastingService.campaign(
        angular.toJson(
          {
            Days: vm.selectedDayCount,
            WorkloadId: vm.selectedWorkload.Workload.Id,
            ScenarioId: vm.selectedScenario.Id,
            CampaignTasksPercent: vm.campaignPercentage
          }), function (data, status, headers, config) {

          }, function (data, status, headers, config) {

          }, function () {
            NoticeService.success($translate.instant('CampaignValuesUpdated'), 5000, true);
            modifyPanelHelper();
            getWorkloadForecastData();
          }
        );
      };

      function checkData(data) {
        if (data == null){
          return false;
        } else {
          return true;
        }
      }

      function applyOverride(form) {
        forecastingService.override(angular.toJson(
          {
            Days: vm.selectedDayCount,
            WorkloadId: vm.selectedWorkload.Workload.Id,
            ScenarioId: vm.selectedScenario.Id,
            OverrideTasks: form.overrideTasksValue,
            OverrideTalkTime: form.overrideTalkTimeValue,
            OverrideAfterCallWork: form.overrideAfterCallWorkValue,
            ShouldSetOverrideTasks: checkData(form.overrideTasksValue),
            ShouldSetOverrideTalkTime: checkData(form.overrideTalkTimeValue),
            ShouldSetOverrideAfterCallWork: checkData(form.overrideAfterCallWorkValue)
          }), function (data, status, headers, config) {

          }, function (data, status, headers, config) {

          }, function () {
            NoticeService.success($translate.instant('OverrideValuesUpdated'), 5000, true);
            modifyPanelHelper();
            getWorkloadForecastData();
          })
        };

        function clearOverride() {
          forecastingService.override(angular.toJson(
            {
              Days: vm.selectedDayCount,
              WorkloadId: vm.selectedWorkload.Workload.Id,
              ScenarioId: vm.selectedScenario.Id,
              ShouldSetOverrideTasks: true,
              ShouldSetOverrideTalkTime: true,
              ShouldSetOverrideAfterCallWork: true
            }), function (data, status, headers, config) {

            }, function (data, status, headers, config) {

            }, function () {
              NoticeService.success($translate.instant('OverrideValuesCleared'), 5000, true);
              modifyPanelHelper();
              getWorkloadForecastData();
              // vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
            })
          };

          function loadChart() {
            return;
          }

          function pointClick(days) {
            vm.selectedDayCount = UniqueArraybyId(days ,"date");
          }

          function UniqueArraybyId(collection, keyname) {
            var output = [],
            keys = [];

            angular.forEach(collection, function(item) {
              var key = item[keyname];
              if(keys.indexOf(key) === -1) {
                keys.push(key);
                output.push(item);
              }
            });
            return output;
          };

          function manageLocalStorage() {
            vm.selectedWorkload = JSON.parse(sessionStorage.currentForecastWorkload);
          }

          function getScenarios() {
            vm.scenarios = [];
            forecastingService.scenarios.query().$promise.then(function (result) {
              result.forEach(function(s) {
                vm.scenarios.push(s);
              });
              changeScenario(vm.scenarios[0]);
            });
          }

          function changeScenario(scenario) {
            vm.selectedScenario = scenario;
            getWorkloadForecastData()
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
                vm.selectedWorkload.Days = data.Days;
                vm.isForecastRunning = false;
                vm.periodModal = false;
                if (vm.selectedWorkload.Days.length === 0 ) {
                  vm.scenarioNotForecasted = true;
                }
              },
              function(data, status, headers, config) {
                vm.selectedWorkload.Days = data.days;
                vm.isForecastRunning = false;
                vm.periodModal = false;
              }
            )
          }

          function forecastWorkload() {
            vm.isForecastRunning = true;
            var temp = {
              WorkloadId: vm.selectedWorkload.Workload.Id,
              ForecastMethodId: -1
            }

            forecastingService.forecast(angular.toJson(
              {
                ForecastStart: vm.forecastPeriod.startDate,
                ForecastEnd: vm.forecastPeriod.endDate,
                Workload: temp,
                ScenarioId: vm.selectedScenario.Id,
                BlockToken: vm.blockToken,
                IsLastWorkload: true
              }), function(data, status, headers, config) {
                vm.isForecastRunning = false;
                vm.forecastModal = false;
                vm.selectedWorkload.Days = data.Days;
              }, function(data, status, headers, config) {
                vm.isForecastRunning = false;
                vm.forecastModal = false;
              });
            }

            function applyWipToScenario() {
              vm.savingToScenario = true;
              var tempForecastDays = [];
              for (var i = 0; i < vm.selectedWorkload.Days.length; i++) {
                tempForecastDays.push(
                  {
                    Date: vm.selectedWorkload.Days[i].date,
                    Tasks: vm.selectedWorkload.Days[i].vtc,
                    TaskTime: vm.selectedWorkload.Days[i].vtt,
                    AfterTaskTime: vm.selectedWorkload.Days[i].vtacw
                  }
                )
              }
              forecastingService.applyToScenario(
                angular.toJson(
                  {
                    WorkloadId: vm.selectedWorkload.Workload.Id,
                    ScenarioId: vm.selectedScenario.Id,
                    ForecastDays: tempForecastDays
                  }
                ), function (data, status, headers, config) {
                  vm.savingToScenario = false;
                }, function(data, status, headers, config) {
                  vm.savingToScenario = false;
                }
              );
            }

            function exportToFile() {
              vm.isForecastRunning = true;
              forecastingService.exportForecast(angular.toJson(
                {
                  ForecastStart: vm.forecastPeriod.startDate,
                  ForecastEnd: vm.forecastPeriod.endDate,
                  ScenarioId: vm.selectedScenario.Id,
                  SkillId: vm.selectedWorkload.SkillId,
                  WorkloadId: vm.selectedWorkload.Workload.Id
                }), function (data, status, headers, config) {
                  var blob = new Blob([data], {
                    type: headers['content-type']
                  });
                  var fileName = moment(vm.forecastPeriod.startDate).format('YYYY-MM-DD') +
                  ' - ' +
                  moment(vm.forecastPeriod.endDate).format('YYYY-MM-DD') +
                  '.xlsx';
                  saveAs(blob, fileName);
                  vm.isForecastRunning = false;
                  vm.exportModal = false;
                }, function(data, status, headers, config) {
                  vm.isForecastRunning = false;
                }
              );
            };

          }
        })();
