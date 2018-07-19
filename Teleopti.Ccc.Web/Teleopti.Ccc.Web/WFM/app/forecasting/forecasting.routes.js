(function() {
  'use strict';

  angular
  .module('wfm.forecasting')
  .config(stateConfig);

  function stateConfig($stateProvider, $urlRouterProvider) {
    $stateProvider.state('forecasting', {
      url: '/forecasting',
      templateUrl: 'app/forecasting/html/forecasting.html',
      controller: function ($state) {
          // toggles.WFM_Forecaster_Refact_44480
          $state.go('forecast');
      }
    })
    .state('forecasting.start', {});

    $stateProvider.state('forecast', {
      url: '/forecast',
      templateUrl: 'app/forecasting/refact/r2forecast.html',
      controller: 'r2ForecastRefactController as vm'
    })
    .state('forecast-modify', {
      url: '/forecast/modify/:workloadId',
      templateUrl: 'app/forecasting/refact/forecast-modify.html',
      controller: 'ForecastModController as vm',
      params: {
        workloadId: undefined
      }
    })
    .state('forecast-create-skill', {
      url: '/forecast/create',
      templateUrl: 'app/forecasting/html/skill-create.html',
      controller: 'ForecastingSkillCreateCtrl'
    });
  }
})();
