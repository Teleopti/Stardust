(function () {
	"use strict";

	angular
	.module("adminApp")
	.controller("etlHistoryController", etlHistoryController, ["$http", "$window"]);

	function etlHistoryController($http, tokenHeaderService, $window) {
		var vm = this;

		var date = new Date();

		vm.historyWorkPeriod = {
			StartDate: new Date(date.getTime() - 86400000),
			EndDate: new Date()
		};
		vm.tenants = [];
		vm.selectedTenant = null;
		vm.selectedBu = null;
		vm.businessUnits = [];

		vm.selectedTenantChanged = selectedTenantChanged;

		(function init() {
			getTenants();
		})();

		function getTenants() {
			vm.tenants = [];
			$http
			.get("./Etl/GetTenants", tokenHeaderService.getHeaders())
			.success(function (data) {
				for (var i = 0; i < data.length; i++) {
					if (data[i].IsBaseConfigured) {
						vm.tenants.push(data[i]);
					} else {
						vm.unconfigured = true;
					}
				}
				vm.tenants.unshift({
					TenantName: '<All>'
				})
				if (!$window.sessionStorage.tenant) {
					vm.selectedTenant = vm.tenants[0];
					selectedTenantChanged();
				} else {
					vm.selectedTenant = getItemBasedOnName(vm.tenants, $window.sessionStorage.tenant, "TenantName");
				}
				getBusinessUnits(vm.selectedTenant.TenantName);
			});
		}

		function getItemBasedOnName(arr, name, prop) {
      for (var i = 0; i < arr.length; i++) {
        if (arr[i][prop] === name) {
          return arr[i];
        }
      }
    }

		function getBusinessUnits(tenant) {
			$http
			.post(
				"./Etl/BusinessUnits",
				JSON.stringify(tenant),
				tokenHeaderService.getHeaders()
			)
			.success(function(data) {
				vm.businessUnits = data;
				vm.selectedBu = vm.businessUnits[0];
			})
			.error(function(data) {
				vm.businessUnits = [];
			});
		}

		function selectedTenantChanged() {
			$window.sessionStorage.tenant = vm.selectedTenant.TenantName;
			getBusinessUnits(vm.selectedTenant.TenantName);
			if (vm.selectedTenant.TenantName === "<All>") {
				vm.selectedBu = getItemBasedOnName(vm.businessUnits, "<All>", "Name");
			}
		}


	}
})();
