(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastModCtrl', ForecastModCtrl);

  ForecastModCtrl.$inject = ['forecastingService', '$stateParams', '$window'];

  function ForecastModCtrl(forecastingService, $stateParams, $window) {
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
    vm.overrideStatus = {
      tasks: false,
      talkTime: false,
      acw: false
    };

    function manageLocalStorage() {
      if ($stateParams.days !== undefined && $stateParams.days.length > 0) {
        $window.localStorage['workload'] = angular.toJson($stateParams);
      }
      storage = angular.fromJson($window.localStorage['workload']);
    };
    manageLocalStorage();

    vm.selectedWorkload = {
      Id: storage.workloadId,
      ChartId: storage.skill.ChartId,
      SkillId: storage.skill.SkillId,
      Days: storage.days,
      Name: storage.skill.Workload.Name
    }

    function resetForms() {
      vm.campaignPercentage = null;
    }

    function modifyPanelHelper(state) {
      if (vm.selectedDayCount === undefined || vm.selectedDayCount.length<1) {
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

    function applyCampaign() {
      forecastingService.campaign(
        JSON.stringify(
          {
            Days: vm.selectedDayCount,
            WorkloadId: vm.selectedWorkload.Id,
            ScenarioId: 'e21d813c-238c-4c3f-9b49-9b5e015ab432',
            CampaignTasksPercent: vm.campaignPercentage
          }), function (data, status, headers, config) {

          }, function (data, status, headers, config) {

          }, function () {
            modifyPanelHelper();
            vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
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
        forecastingService.override(JSON.stringify(
          {
            Days: vm.selectedDayCount,
            WorkloadId: vm.selectedWorkload.Id,
            ScenarioId: 'e21d813c-238c-4c3f-9b49-9b5e015ab432',
            OverrideTasks: form.overrideTasksValue,
            OverrideTalkTime: form.overrideTalkTimeValue,
            OverrideAfterCallWork: form.overrideAfterCallWorkValue,
            ShouldSetOverrideTasks: checkData(form.overrideTasksValue),
            ShouldSetOverrideTalkTime: checkData(form.overrideTalkTimeValue),
            ShouldSetOverrideAfterCallWork: checkData(form.overrideAfterCallWorkValue)
          }), function (data, status, headers, config) {

          }, function (data, status, headers, config) {

          }, function () {
            modifyPanelHelper();
            vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
          })
        };

        function loadChart(chartId, days) {
          //placeholder function
        }

        function pointClick(days) {
          vm.selectedDayCount = days;
        }
      }

    })();
