(function() {
  'use strict';

  angular
  .module('wfm.resourceplanner')
  .config(stateConfig);

  function stateConfig($stateProvider) {
    $stateProvider.state('resourceplanner', {
      url: '/resourceplanner',
      templateUrl: 'app/resourceplanner/resourceplanner.html',
      controller: 'ResourcePlannerCtrl'
    }).state('resourceplanner.filter', {
      params: {
        filterId: {},
        periodId: {},
        isDefault: {}
      },
      url: '/dayoffrules',
      templateUrl: 'app/resourceplanner/resourceplanner-filters.html',
      controller: 'ResourceplannerFilterCtrl'
    }).state('resourceplanner.planningperiod', {
      url: '/planningperiod/:id',
      templateUrl: 'app/resourceplanner/planningperiods.html',
      controller: 'PlanningPeriodsCtrl'
    }).state('resourceplanner.report', {
      params: {
        result: {},
        interResult: [],
        planningperiod: {}
      },
      url: '/report/:id',
      templateUrl: 'app/resourceplanner/resourceplanner-report.html',
      controller: 'ResourceplannerReportCtrl'
    }).state('resourceplanner.temp', {
      url: '/optimize/:id',
      templateUrl: 'app/resourceplanner/temp.html',
      controller: 'ResourceplannerTempCtrl'
    })
  }
})();
