(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .controller('ForecastModCtrl', ForecastModCtrl);

  ForecastModCtrl.$inject = ['forecastingService', '$stateParams'];

  function ForecastModCtrl(forecastingService, $stateParams) {
    var vm = this;
    vm.selectedWorkload = $stateParams.workloadId;
  }
})();
