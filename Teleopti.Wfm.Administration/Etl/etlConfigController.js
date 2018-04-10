(function() {
  "use strict";

  angular
  .module("adminApp")
  .controller("etlConfigController", etlConfigController, [
    "$http",
    "$timeout",
    "$window"
  ]);

  function etlConfigController($http, tokenHeaderService, $timeout, $window) {
    var vm = this;

    vm.tenants = [];
    vm.tenantLogData = null;
    vm.TimeZoneCodes = null;
    vm.selectedTenant = null;

    vm.sendLogDataConfiguration = sendLogDataConfiguration;
	  vm.getLogDataForATenant = getLogDataForATenant;
	  vm.getTenants = getTenants;

    (function init() {
      getTenants();
    })();

    function getTenants() {
      $http
      .get("./Etl/GetTenants", tokenHeaderService.getHeaders())
      .success(function(data) {
        vm.tenants = data;
        // addUrl();
      });
    }

    // function addUrl() {
    //   for (var i = 0; i < vm.tenants.length; i++) {
    //     vm.tenants[i].Url = encodeURIComponent(vm.tenants[i].TenantName)
    //   }
    // }

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
        if ($window.sessionStorage.configData) {
          var temp = angular.fromJson($window.sessionStorage.configData);
          vm.TimeZoneCodes = temp.TimeZoneList;
          tenant.loading = null;
          vm.showModal = true;
        } else {
          $http.get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
          .success(function (timezones) {
            vm.TimeZoneCodes = timezones.TimeZoneList;
            tenant.loading = null;
            vm.showModal = true;
            $window.sessionStorage.configData = angular.toJson(data);
          });
        }
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
