(function () {
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

		vm.SkipConflicts = true;

		vm.CheckTenantName = function () {
			$http.post('../api/Import/IsNewTenant', '"' + vm.Tenant + '"')
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					vm.CheckUsers();

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
			if (vm.AppDbOk == false) {
				vm.Conflicts = null;
				return;
			}
			$http.post('../api/Import/Conflicts', {
				ConnStringAppDatabase: vm.AppDatabase,
				UserPrefix: vm.UserPrefix
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
				ConnStringAnalyticsDatabase: vm.AnalyticsDatabase,
				SkipConflicts: vm.SkipConflicts,
				UserPrefix: vm.UserPrefix
			}).
  success(function (data) {
  	vm.Result = data.Success;
  	vm.Message = data.Message;
  }).
  error(function (xhr, ajaxOptions, thrownError) {
  	console.log(xhr.status + xhr.responseText + thrownError);
  });
		};
		//$http.get('../api/Home/GetAllTenants').success(function (data) {
		//	$scope.Tenants = data;
		//}).error(function (xhr, ajaxOptions, thrownError) {
		//	console.log(xhr.status + xhr.responseText + thrownError);
		//});
	}

})();