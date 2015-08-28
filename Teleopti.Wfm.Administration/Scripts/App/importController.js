﻿(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('importController', importController, []);

	function importController($http) {
		var vm = this;
		var tokenKey = 'accessToken';
		vm.Tenant = "";
		vm.Server = "";
		vm.UserName = "";
		vm.Password = "";
		vm.AppDatabase = "";
		vm.AnalyticsDatabase = "";
		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;
		vm.AppDbOk = false;
		vm.AppDbCheckMessage = "Input Application database";

		vm.AnalDbOk = false;
		vm.AnalDbCheckMessage = "Input Analytics database";
		vm.HeadVersion = null;
		vm.ImportAppVersion = null;
		vm.AppVersionOk = null;

		vm.token = sessionStorage.getItem(tokenKey);
		if (vm.token === null) {
			return;
		}

		function getHeaders() {
			return {
				headers: { 'Authorization': 'Bearer ' + vm.token }
			};
		}

		vm.CheckTenantName = function () {
			$http.post('./api/Import/IsNewTenant', '"' + vm.Tenant + '"', getHeaders())
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					vm.CheckUsers();

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
			}, getHeaders())
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
			}, getHeaders())
				.success(function (data) {
					vm.AnalDbOk = data.Exists;
					vm.AnalDbCheckMessage = data.Message;
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
			}, getHeaders())
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
				ConnStringAppDatabase: vm.AppDatabase,
				Tenant: vm.Tenant
			}, getHeaders())
				.success(function (data) {
					vm.Conflicts = data.ConflictingUserModels;
					vm.NumberOfConflicting = data.NumberOfConflicting;
					vm.NumberOfNotConflicting = data.NumberOfNotConflicting;

				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.startImport = function () {
			$http.post('./api/Import/ImportExisting', {
				Tenant: vm.Tenant,
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				AnalyticsDatabase: vm.AnalyticsDatabase,
				AppDatabase: vm.AppDatabase,
			}, getHeaders())
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