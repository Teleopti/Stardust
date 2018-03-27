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
      tenant.loading = true;
      $http
      .post(
        "./Etl/TenantAllLogDataSources",
        JSON.stringify(tenant.TenantName),
        tokenHeaderService.getHeaders()
      )
      .success(function(data) {
        vm.tenantLogData = data;

        $http
        .get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
        .success(function(timezones) {
          vm.TimeZoneCodes = timezones.TimeZoneList;
          tenant.loading = null;
          vm.showModal = true;
        });
      });
    }

    // vm.sendAllLogDataForATenant = sendAllLogDataForATenant;
    // function sendAllLogDataForATenant(tenant) {
    //   $http
    //   .post(
    //     "./Etl/Jobs",
    //     JSON.stringify(tenant),
    //     tokenHeaderService.getHeaders()
    //   )
    //   .success(function(data) {
    //
    //   })
    //   .error(function(data) {
    //
    //   });
    // }

  }
})();
