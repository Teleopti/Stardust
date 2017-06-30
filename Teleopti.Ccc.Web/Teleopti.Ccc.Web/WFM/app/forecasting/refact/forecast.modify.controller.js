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

    function loadChart(chartId, days) {
      //placeholder function
    }

    function pointClick(days) {
      vm.selectedDayCount = days;
    }
  }

})();
