(function() {
  'use strict';

  angular
  .module('wfm.resourceplanner')
  .config(stateConfig);

  function stateConfig($stateProvider) {
    $stateProvider.state('resourceplanner', {
      url: '/resourceplanner',
      templateUrl: 'js/resourceplanner/resourceplanner.html',
      controller: 'ResourcePlannerCtrl'
    }).state('resourceplanner.filter', {
      params: {
        filterId: {},
        periodId: {},
        isDefault: {}
      },
      url: '/dayoffrules',
      templateUrl: 'js/resourceplanner/resourceplanner-filters.html',
      controller: 'ResourceplannerFilterCtrl'
    }).state('resourceplanner.planningperiod', {
      url: '/planningperiod/:id',
      templateUrl: 'js/resourceplanner/planningperiods.html',
      controller: 'PlanningPeriodsCtrl'
    }).state('resourceplanner.report', {
      params: {
        result: {},
        interResult: [],
        planningperiod: {}
      },
      url: '/report/:id',
      templateUrl: 'js/resourceplanner/resourceplanner-report.html',
      controller: 'ResourceplannerReportCtrl'
    }).state('resourceplanner.temp', {
      url: '/optimize/:id',
      templateUrl: 'js/resourceplanner/temp.html',
      controller: 'ResourceplannerTempCtrl'
    })
  }
})();
