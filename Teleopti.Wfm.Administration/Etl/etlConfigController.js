(function() {
  "use strict";

  angular
  .module("adminApp")
  .controller("etlConfigController", etlConfigController, [
    "$http",
    "$timeout"
  ]);

  function etlConfigController($http, tokenHeaderService, $timeout) {
    var vm = this;

    vm.tenants = [];
    vm.tenantLogData = null;
    vm.TimeZoneCodes = null;
    vm.selectedTenant = null;

    vm.sendLogDataConfiguration = sendLogDataConfiguration;
    vm.getLogDataForATenant = getLogDataForATenant;

    (function init() {
      getTenants();
    })();

    function getTenants() {
      $http
      .get("./Etl/GetTenants", tokenHeaderService.getHeaders())
      .success(function(data) {
        vm.tenants = data;
        addUrl();
      });
    }

    function addUrl() {
      for (var i = 0; i < vm.tenants.length; i++) {
        vm.tenants[i].Url = encodeURIComponent(vm.tenants[i].TenantName)
      }
    }

    function getLogDataForATenant(tenant) {
      vm.selectedTenant = tenant;
      tenant.loading = true;
      $http
      .post(
        "./Etl/TenantAllLogDataSources",
        JSON.stringify(tenant.TenantName),
        tokenHeaderService.getHeaders()
      )
      .success(function(data) {
        vm.tenantLogData = data;
        console.log(data);

        $http
        .get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
        .success(function(timezones) {
          vm.TimeZoneCodes = timezones.TimeZoneList;
          tenant.loading = null;
          vm.showModal = true;
        });
      });
    }

    function sendLogDataConfiguration() {
      vm.tenantLogData.Error = null;
      var logDataObject = {
        TenantName: vm.selectedTenant.TenantName,
        DataSources: vm.tenantLogData
      }
      vm.logDataSending = true;
      $http
      .post(
        "./Etl/PersistDataSource",
        JSON.stringify(logDataObject),
        tokenHeaderService.getHeaders()
      )
      .success(function(data) {
        vm.tenantLogData = null;
        vm.showModal = false;
        vm.selectedTenant = null;
        vm.logDataSending = false;
      })
      .error(function(data) {
        vm.logDataSending = false;
        vm.tenantLogData.Error = data;
      });
    }

  }
})();
