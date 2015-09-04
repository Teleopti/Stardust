(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('importController', importController, ['tokenHeaderService']);

	function importController($http, tokenHeaderService) {
		var vm = this;

		vm.Tenant = "";
		vm.Server = "";
		vm.UserName = "";
		vm.Password = "";
		vm.AppDatabase = "";
		vm.AnalyticsDatabase = "";
		vm.AggDatabase = "";
		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;
		vm.AppDbOk = false;
		vm.AppDbCheckMessage = "Input Application database";

		vm.AnalDbOk = false;
		vm.AnalDbCheckMessage = "Input Analytics database";
		
		vm.AggDbOk = false;
		vm.AggDbCheckMessage = "Input Aggregation database (this is optional)";

		vm.HeadVersion = null;
		vm.ImportAppVersion = null;
		vm.AppVersionOk = true;

		vm.CreateDbUser = '';
		vm.CreateDbPassword = '';
		vm.SqlUserOkMessage = '';
		vm.SqlUserOk = false;

		vm.CheckTenantName = function () {
			$http.post('./api/Import/IsNewTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					vm.CheckUsers();

				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckImportAdmin = function () {
			vm.Message = '';
			if (vm.CreateDbUser === '' || vm.CreateDbPassword === '' || vm.Server === '') {
				vm.SqlUserOkMessage = '';
				vm.SqlUserOk = false;
				return;
			}
			var model = {
				Server: vm.Server,
				AdminUser: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword
			}

			$http.post('./CheckImportAdmin', model, tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.SqlUserOk = data.Success,
				vm.SqlUserOkMessage = data.Message;
				vm.CheckLogin();

			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.status + xhr.responseText + thrownError);
			});
		}

		vm.CheckAppDb = function () {
			$http.post('./DbExists', {
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				Database: vm.AppDatabase,
				DbType: 1
			}, tokenHeaderService.getHeaders())
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
			$http.post('./DbExists', {
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				Database: vm.AnalyticsDatabase,
				DbType: 2
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.AnalDbOk = data.Exists;
					vm.AnalDbCheckMessage = data.Message;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckAggDb = function () {
			if (vm.AggDatabase === "") {
				vm.AggDbOk = false;
				vm.AggDbCheckMessage = "Input Aggregation database (this is optional)";
				return;
			}
			$http.post('./DbExists', {
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				Database: vm.AggDatabase,
				DbType: 3
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.AggDbOk = data.Exists;
					vm.AggDbCheckMessage = data.Message;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckVersions = function () {
			$http.post('./api/UpgradeDatabases/GetVersions', {
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				AppDatabase: vm.AppDatabase
			}, tokenHeaderService.getHeaders())
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
			$http.post('./api/Import/Conflicts', {
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				AppDatabase: vm.AppDatabase,
				Tenant: vm.Tenant
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.Conflicts = data.ConflictingUserModels;
					vm.NumberOfConflicting = data.NumberOfConflicting;
					vm.NumberOfNotConflicting = data.NumberOfNotConflicting;

				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.startImport = function () {
			if (vm.AppVersionOk != true && vm.SqlUserOk != true) {
				alert("When importing an older version you must provide a valid account to upgrade the databases.");
				return;
			}
			$http.post('./api/Import/ImportExisting', {
				Tenant: vm.Tenant,
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				AnalyticsDatabase: vm.AnalyticsDatabase,
				AppDatabase: vm.AppDatabase,
				AggDatabase: vm.AggDatabase,
				AdminUser: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.Success = data.Success;
					vm.Message = data.Message;
				})
				.error(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.status + xhr.responseText + thrownError;
					vm.Success = false;
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		};
	}

})();