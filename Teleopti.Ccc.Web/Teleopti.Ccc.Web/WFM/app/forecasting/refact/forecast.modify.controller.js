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
    vm.applyOverride = applyOverride;
    vm.applyCampaign = applyCampaign;
    vm.clearCampaign = clearCampaign;
    vm.clearOverride = clearOverride;
    vm.isForecastRunning = false;
    vm.overrideStatus = {
      tasks: false,
      talkTime: false,
      acw: false
    };

    manageLocalStorage();

    function resetForms() {
      vm.campaignPercentage = null;
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

    function applyCampaign() {
      forecastingService.campaign(
        angular.toJson(
          {
            Days: vm.selectedDayCount,
            WorkloadId: vm.selectedWorkload.Id,
            ScenarioId: vm.selectedWorkload.Scenario.Id,
            CampaignTasksPercent: vm.campaignPercentage
          }), function (data, status, headers, config) {

          }, function (data, status, headers, config) {

          }, function () {
            NoticeService.success($translate.instant('CampaignValuesUpdated'), 5000, true);
            modifyPanelHelper();
            refreshOnModify();
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
            WorkloadId: vm.selectedWorkload.Id,
            ScenarioId: vm.selectedWorkload.Scenario.Id,
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
            refreshOnModify();
          })
        };

        function clearOverride() {
          forecastingService.override(angular.toJson(
            {
              Days: vm.selectedDayCount,
              WorkloadId: vm.selectedWorkload.Id,
              ScenarioId: vm.selectedWorkload.Scenario.Id,
              ShouldSetOverrideTasks: true,
              ShouldSetOverrideTalkTime: true,
              ShouldSetOverrideAfterCallWork: true
            }), function (data, status, headers, config) {

            }, function (data, status, headers, config) {

            }, function () {
              NoticeService.success($translate.instant('OverrideValuesCleared'), 5000, true);
              modifyPanelHelper();
              refreshOnModify();
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

          function refreshOnModify() {
            vm.isForecastRunning = true;
            var wl = {
              ForecastStart: moment().utc().add(1, 'months').startOf('month').toDate(),
              ForecastEnd: moment().utc().add(2, 'months').startOf('month').toDate(),
              WorkloadId: vm.selectedWorkload.Id,
              ScenarioId: vm.selectedWorkload.Scenario.Id
            };

            forecastingService.result(
              wl,
              function(data, status, headers, config) {
                vm.selectedWorkload.Days = data.Days;
                vm.isForecastRunning = false;
                vm.loadChart('chart'+ vm.selectedWorkload.Id, data.Days);
              }
            )
          }

          function manageLocalStorage() {
            vm.selectedWorkload = {
              Id: $stateParams.workloadId,
              ChartId: $stateParams.skill.ChartId,
              SkillId: $stateParams.skill.SkillId,
              Days: $stateParams.days,
              Name: $stateParams.skill.Workload.Name,
              Scenario: $stateParams.scenario
            }
          };
        }

      })();
