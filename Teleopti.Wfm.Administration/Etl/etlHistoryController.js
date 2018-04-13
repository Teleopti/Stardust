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
		vm.history = [];
		vm.error = null;

		vm.selectedTenantChanged = selectedTenantChanged;
		vm.getHistoryForTenant = getHistoryForTenant;

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

		function getHistoryForTenant() {
			if (!vm.selectedTenant || !vm.selectedBu) {
				return;
			}

			var	JobHistoryCriteria = {
				BusinessUnitId: vm.selectedBu.Id,
				TenantName: vm.selectedTenant.TenantName,
				StartDate: vm.historyWorkPeriod.StartDate,
				EndDate: vm.historyWorkPeriod.EndDate,
				ShowOnlyErrors: false
			};

			$http
			.post(
				"./Etl/GetJobHistory",
				JSON.stringify(JobHistoryCriteria),
				tokenHeaderService.getHeaders()
			)
			.success(function(data) {
				if (data.length < 1) {
					vm.error = "No history found";
				} else {
					vm.history = data;
					vm.error = null;
				}
			})
			.error(function(data) {
				vm.history = [];
				vm.error = "No history found";
			});
		}

	}
})();
