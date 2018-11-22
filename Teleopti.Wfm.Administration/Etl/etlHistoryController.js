(function () {
	"use strict";

	angular
	.module("adminApp")
	.controller("etlHistoryController", etlHistoryController, ["$http", "$window", "$timeout"]);

	function etlHistoryController($http, tokenHeaderService, $window, $timeout) {
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
		vm.status = null;
		vm.loadingHistory = false;

		vm.selectedTenantChanged = selectedTenantChanged;
		vm.buildError = buildError;
		vm.getHistoryForTenant = getHistoryForTenant;
		vm.getStatusRightNow = getStatusRightNow;
		vm.copy = copy;

		(function init() {
			checkToggle();
			getTenants();
			vm.getStatusRightNow();
		})();

		function checkToggle() {
			$http.get("./Etl/ShouldTenantNameBeVisible", tokenHeaderService.getHeaders())
				.success(function(result) {
					vm.shouldShowTenantName = result;
				});
		}

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
				});
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
				vm.getHistoryForTenant();
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

		function pollStatus() {
			$timeout( function(){
				$http
				.get("./Etl/GetjobRunning", tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.status = data;
					if (vm.status !== null) {
						vm.status.formatedTime = moment(vm.status.StartTime).local().format('HH:mm');
					}
					pollStatus();
				});
			}, 10000 );
		}

		function getStatusRightNow() {
				$http
				.get("./Etl/GetjobRunning", tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.status = data;
					if (vm.status !== null) {
						vm.status.formatedTime = moment(vm.status.StartTime).local().format('HH:mm');
					}
					pollStatus();
				});
		}

		function getHistoryForTenant() {
			if (!vm.selectedTenant || !vm.selectedBu) {
				return;
			}

			vm.loadingHistory = true;

			var	JobHistoryCriteria = {
				BusinessUnitId: vm.selectedBu.Id,
				TenantName: vm.selectedTenant.TenantName,
				StartDate: vm.historyWorkPeriod.StartDate,
				EndDate: vm.historyWorkPeriod.EndDate,
				ShowOnlyErrors: vm.errorsOnly
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
				}).then(function() {
					vm.loadingHistory = false;
				});
		}

		function buildError(root) {
			root.ConstructedError = null;
			root.ConstructedError = 'EXCEPTION MESSAGE \n' +
root.ErrorMessage + '\n' +
'=========================== \n' +
'EXCEPTION STACKTRACE \n' +
root.ErrorStackTrace + '\n' +
'=========================== \n' +
'INNER EXCEPTION MESSAGE \n' +
root.InnerErrorMessage + '\n' +
'=========================== \n' +
'INNER EXCEPTION STACKTRACE \n' +
root.InnerErrorStackTrace + '\n' +
'=========================== \n';
			console.log(root);
		}

		function copy(root) {
			buildError(root);
			root.Copied = true;
			$timeout( function(){
				root.Copied = false;
			}, 5000 );
		}

	}
})();
