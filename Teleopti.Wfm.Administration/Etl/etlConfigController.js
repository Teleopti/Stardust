(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("etlConfigController", etlConfigController, [
			"$http",
			"$timeout",
			"$window"
		]);

	function etlConfigController($http, $timeout, $window) {
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
				.get("./Etl/GetTenants")
				.then(function (response) {
					vm.tenants = response.data;
					// addUrl();
				});
		}

		function getLogDataForATenant(tenant) {
			vm.selectedTenant = tenant;
			tenant.loading = true;
			$http
				.post(
					"./Etl/TenantLogDataSources",
					JSON.stringify(tenant.TenantName)
				)
				.then(function (response) {
					vm.tenantLogData = response.data;
					if ($window.sessionStorage.configData) {
						var stringifiedConfigData = angular.fromJson($window.sessionStorage.configData);
						vm.TimeZoneCodes = stringifiedConfigData.TimeZoneList;
						tenant.loading = null;
						vm.showModal = true;
					} else {
						$http.get("./Etl/GetConfigurationModel")
							.then(function (response) {
								$window.sessionStorage.configData = angular.toJson(response.data)
								var stringifiedConfigData = angular.fromJson($window.sessionStorage.configData);
								vm.TimeZoneCodes = stringifiedConfigData.TimeZoneList;
								tenant.loading = null;
								vm.showModal = true;
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
					JSON.stringify(logDataObject)
				)
				.then(function (response) {
					vm.tenantLogData = null;
					vm.showModal = false;
					vm.selectedTenant = null;
					vm.logDataSending = false;
				})
				.catch(function (response) {
					vm.logDataSending = false;
					vm.tenantLogData.Error = response.data;
				});
		}

	}
})();
