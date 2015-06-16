﻿(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('importController', importController, []);

	function importController($http) {
		var vm = this;
		vm.Tenant = "";
		vm.AppDatabase = "";
		vm.AnalyticsDatabase = "";
		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;
		vm.AppDbOk = false;
		vm.AppDbCheckMessage = "Input connection string";

		vm.AnalDbOk = false;
		vm.AnalDbCheckMessage = "Input connection string";
		vm.HeadVersion = null;
		vm.ImportAppVersion = null;
		vm.AppVersionOk = null;

		//api/Import/TenantExists

		vm.CheckTenantName = function () {
			$http.post('../api/Import/IsNewTenant', '"' + vm.Tenant + '"')
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;

				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckAppDb = function () {
			$http.post('../api/Import/DbExists', {
				DbConnectionString: vm.AppDatabase,
				DbType: 1
			})
				.success(function (data) {
					vm.AppDbOk = data.Exists;
					vm.AppDbCheckMessage = data.Message;
					if (vm.AppDbOk) {
						vm.CheckVersions();
						vm.CheckUsers();
					}
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}
		vm.CheckAnalDb = function () {
			$http.post('../api/Import/DbExists', {
				DbConnectionString: vm.AnalyticsDatabase,
				DbType: 2
			})
				.success(function (data) {
					vm.AnalDbOk = data.Exists;
					vm.AnalDbCheckMessage = data.Message;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckVersions = function () {
			$http.post('../api/UpgradeDatabases/GetVersions', {
				AppConnectionString: vm.AppDatabase
			})
				.success(function (data) {
					vm.HeadVersion = data.HeadVersion;
					vm.ImportAppVersion = data.ImportAppVersion;
					vm.AppVersionOk = data.AppVersionOk;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}
		vm.CheckUsers = function () {
			$http.post('../api/Import/Conflicts', {
				ConnStringAppDatabase: vm.AppDatabase
			})
				.success(function (data) {
					vm.Conflicts = data.ConflictingUserModels;
					vm.NumberOfConflicting = data.NumberOfConflicting;
					vm.NumberOfNotConflicting = data.NumberOfNotConflicting;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.startImport = function () {

			$http.post('../api/Import/ImportExisting', {
				Tenant: vm.Tenant,
				ConnStringAppDatabase: vm.AppDatabase,
				ConnStringAnalyticsDatabase: vm.AnalyticsDatabase
			}).
  success(function (data, status, headers, config) {
  	vm.Result = data.Success;
  	vm.Message = data.Message;
  }).
  error(function (data, status, headers, config) {
  	// called asynchronously if an error occurs
  	// or server returns response with an error status.
  });
		};
		//$http.get('../api/Home/GetAllTenants').success(function (data) {
		//	$scope.Tenants = data;
		//}).error(function (xhr, ajaxOptions, thrownError) {
		//	console.log(xhr.status + xhr.responseText + thrownError);
		//});
	}

})();