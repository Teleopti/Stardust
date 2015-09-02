(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('detailsController', detailsController, ['tokenHeaderService']);

	function detailsController($http, $routeParams, tokenHeaderService) {
		var vm = this;

		vm.Tenant = $routeParams.tenant;
		vm.OriginalName = $routeParams.tenant;
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
		vm.CommandTimeout = 0;
		vm.Message = "";
		vm.AllowDelete = false;
		
		vm.LoadTenant = function () {
			$http.post('./GetOneTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					vm.Server = data.Server;
					vm.UserName = data.UserName;
					vm.Password = data.Password;
					vm.AnalyticsDatabase = data.AnalyticsDatabase;
					vm.AppDatabase = data.AppDatabase;
					vm.CommandTimeout = data.CommandTimeout;
					vm.CheckAppDb();
					vm.CheckAnalDb();
					vm.CheckDelete();
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckTenantName = function () {
			$http.post('./api/Home/NameIsFree', {
				OriginalName: vm.OriginalName,
				NewName: vm.Tenant,
				AppDatabase: vm.AppDatabase,
				AnalyticsDatabase: vm.AnalyticsDatabase
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckDelete = function () {
			$http.post('./TenantCanBeDeleted', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.AllowDelete = data.Success;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.Delete = function () {
			$http.post('./DeleteTenant', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckAppDb = function () {
			vm.AppDbOk = false;
			vm.AppDbCheckMessage = "Checking database.....";
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
					//display how many maybe
					//if (vm.AppDbOk) {
					//	vm.CheckUsers();
					//}
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}
		vm.CheckAnalDb = function () {
			vm.AnalDbOk = false;
			vm.AnalDbCheckMessage = "Checking database.....";
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
		vm.LoadTenant();
		vm.save = function () {
			if (vm.AppDbOk === false) {
				alert("Fix the settings to the Application database.");
				return;
			}
			if (vm.AnalDbOk === false) {
				alert("Fix the settingsto the Analytics database.");
				return;
			}
			if (vm.TenantOk === false) {
				alert("The new name of the Tenant must be changed before saving.");
				return;
			}

			$http.post('./UpdateTenant', {
				OriginalName: vm.OriginalName,
				NewName: vm.Tenant,
				AppDatabase: vm.AppDatabase,
				AnalyticsDatabase: vm.AnalyticsDatabase,
				Server: vm.Server,
				UserName: vm.UserName,
				Password: vm.Password,
				CommandTimeout: vm.CommandTimeout
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				})
				.error(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.status + xhr.responseText + thrownError;
					vm.Success = false;
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		};
	}

})();