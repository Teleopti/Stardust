(function () {
  'use strict';

  angular.module('adminApp')
  .component('etlScheduleModal',
  {
    templateUrl: './Etl/scheduleModal.html',
    controller: ['$http', 'tokenHeaderService', etlScheduleModal],
    controllerAs: 'ctrl',
    bindings: {
      job: '<',
      output: '=',
      callback: '='
    }
  });

  function etlScheduleModal($http, tokenHeaderService) {
    var ctrl = this;

    ctrl.selectedTenantChanged = selectedTenantChanged;
    ctrl.toggleFrequencyType = toggleFrequencyType;
    ctrl.selectedjobChanged= selectedjobChanged;
    ctrl.isRelativePeriodNeeded = isRelativePeriodNeeded;



    ctrl.frequencyType = false;
    ctrl.tenantLogData = [];

    (function init() {
      getTenantsForModal();
    })();

    function getJobs(tenant) {
      $http
      .post(
        "./Etl/Jobs",
        JSON.stringify(tenant),
        tokenHeaderService.getHeaders()
      )
      .success(function(data) {
        ctrl.jobs = data;
      })
      .error(function(data) {
        ctrl.jobs = [];
      });
    }

    function getTenantsForModal() {
      ctrl.tenants = [];
      $http
      .get("./Etl/GetTenants", tokenHeaderService.getHeaders())
      .success(function (data) {
        for (var i = 0; i < data.length; i++) {
          if (data[i].IsBaseConfigured) {
            ctrl.tenants.push(data[i]);
          }
        }
        ctrl.selectedTenant = ctrl.tenants[0];
        selectedTenantChanged(ctrl.selectedTenant);
      });
    }

    function toggleFrequencyType(form) {
      if (ctrl.frequencyType) {
        form.DailyFrequencyStart = null;
      }
      else{
        form.DailyFrequencyMinute = null;
        form.DailyFrequencyStart = null;
        form.DailyFrequencyEnd = null;
      }
    }

    function getLogDataForATenant(tenant) {
      ctrl.tenantLogData = null;
      $http
      .post(
        "./Etl/TenantAllLogDataSources",
        JSON.stringify(tenant),
        tokenHeaderService.getHeaders()
      )
      .success(function(data) {
        ctrl.tenantLogData = data;
      });
    }

    function selectedTenantChanged(tenant){
      ctrl.selectedTenant = tenant;
      getJobs(ctrl.selectedTenant.TenantName);
      getLogDataForATenant(ctrl.selectedTenant.TenantName);
    }

    function selectedjobChanged(form){
      form.LogDataSourceId = null;
    }

    function isRelativePeriodNeeded(name, arr) {
      if (!arr) {
        return true;
      }

      return !arr.includes(name);
    }

  }
})();
