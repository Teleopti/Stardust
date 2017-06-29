(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastModCtrl', ForecastModCtrl);

  ForecastModCtrl.$inject = ['forecastingService', '$stateParams'];

  function ForecastModCtrl(forecastingService, $stateParams) {
    var vm = this;
    console.log($stateParams);
    vm.selectedWorkload = {
      Id: $stateParams.workloadId,
      ChartId: $stateParams.skill.ChartId,
      SkillId: $stateParams.skill.SkillId,
      Days: $stateParams.days,
      Name: $stateParams.skill.Workload.Name
    }

    vm.loadChart = loadChart;
    vm.pointClick = pointClick;

    function loadChart(chartId, days) {
      //placeholder function
    }

    function pointClick(days) {
      vm.selectedDayCount = days;
    }

    vm.loadChart(vm.selectedWorkload.ChartId, vm.selectedWorkload.Days);
  }
})();
