(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('detailsController', detailsController, []);

	function detailsController($http, $routeParams) {
		var vm = this;
		vm.Tenant = $routeParams.tenant;
		vm.OriginalName = $routeParams.tenant;
		vm.AppDatabase = "";
		vm.AnalyticsDatabase = "";
		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;
		vm.AppDbOk = false;
		vm.AppDbCheckMessage = "Input connection string";

		vm.AnalDbOk = false;
		vm.AnalDbCheckMessage = "Input connection string";
		
		vm.LoadTenant = function () {
			$http.post('./api/Home/GetOneTenant', '"' + vm.Tenant + '"')
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					vm.AnalyticsDatabase = data.AnalyticsDatabase;
					vm.AppDatabase = data.AppDatabase;
					vm.CheckAppDb();
					vm.CheckAnalDb();
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
			})
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckAppDb = function () {
			vm.AppDbOk = false;
			vm.AppDbCheckMessage = "Checking database.....";
			$http.post('./api/Import/DbExists', {
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
			vm.AnalDbOk = false;
			vm.AnalDbCheckMessage = "Checking database.....";
			$http.post('./api/Import/DbExists', {
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
		vm.LoadTenant();
		vm.save = function () {
			if (vm.AppDbOk === false) {
				alert("Fix the connections string to the Application database.");
				return;
			}
			if (vm.AnalDbOk === false) {
				alert("Fix the connections string to the Analytics database.");
				return;
			}
			if (vm.TenantOk === false) {
				alert("The new name of the Tenant must be changed before saving.");
				return;
			}
			
			$http.post('./api/Home/Save', {
				OriginalName: vm.OriginalName,
				NewName: vm.Tenant,
				AppDatabase: vm.AppDatabase,
				AnalyticsDatabase: vm.AnalyticsDatabase
			})
				.success(function (data) {
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