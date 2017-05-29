(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .config(stateConfig);

  function stateConfig($stateProvider) {
    $stateProvider.state('forecasting', {
      url: '/forecasting',
      templateUrl: 'app/forecasting/html/forecasting.html',
      controller: 'ForecastingDefaultCtrl',
      resolve: {
        toggles: function (Toggle) {
          return Toggle;
        }
      },
      controller: function ($state, toggles) {
        if (toggles.WFM_Forecaster_Refact_44480) {
          $state.go('forecast');
        } else {
          $state.go('forecasting.start');
        }
      }
    })
    .state('forecast', {
      params: {
        workloadId: undefined
      },
      templateUrl: 'app/forecasting/refact/forecast.html',
      controller: ''
    })
    .state('forecasting.start', {
      params: {
        workloadId: undefined
      },
      templateUrl: 'app/forecasting/html/forecasting-overview.html',
      controller: 'ForecastingStartCtrl'
    })
    .state('forecasting.advanced', {
      url: '/advanced/:workloadId',
      templateUrl: 'app/forecasting/html/forecasting-advanced.html',
      controller: 'ForecastingAdvancedCtrl'
    })
    .state('forecasting.skillcreate', {
      url: '/skill/create',
      templateUrl: 'app/forecasting/html/skill-create.html',
      controller: 'ForecastingSkillCreateCtrl'
    })
  }
})();
