(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('importController', importController, ['tokenHeaderService']);

	function importController($http, tokenHeaderService) {
		var vm = this;
		$("#loading").hide();
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
		vm.ImportAppVersion = 0;
		vm.AppVersionOk = false;
		vm.ToEarly = true;
		vm.ToEarlyVersionMessage = '';

		vm.CreateDbUser = '';
		vm.CreateDbPassword = '';
		vm.SqlUserOkMessage = '';
		vm.SqlUserOk = false;

		vm.AppLoginMessage = '';
		vm.AppLoginOk = false;

		vm.ShowTheLog = false;
		vm.ShowLog = false;
		vm.ShowHide = function() {
			//If DIV is visible it will be hidden and vice versa.
			vm.ShowTheLog = vm.ShowLog;
		};

		vm.CheckTenantName = function() {
			$http.post('./api/Import/IsNewTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.then(function(response) {
					vm.TenantMessage = response.data.Message;
					vm.TenantOk = response.data.Success;
					//vm.CheckUsers();

				});
		};

		vm.CheckImportAdmin = function() {
			vm.Message = '';
			if (vm.CreateDbUser === '' || vm.CreateDbPassword === '' || vm.Server === '') {
				vm.SqlUserOkMessage = '';
				vm.SqlUserOk = false;
				return;
			}
			var model = {
				Server: vm.Server,
				AdminUser: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UseIntegratedSecurity: false
			};

			$http.post('./CheckImportAdmin', model, tokenHeaderService.getHeaders())
				.then(function(response) {
					vm.SqlUserOk = response.data.Success,
						vm.SqlUserOkMessage = response.data.Message;
				});
		};

		vm.CheckAppLogin = function() {
			vm.Message = '';
			if (vm.SqlUserOk === false) {
				vm.AppLoginMessage = '';
				vm.AppLoginOk = false;
				return;
			}
			if (vm.UserName === '' || vm.Password === '') {
				vm.AppLoginOk = false;
				vm.AppLoginMessage = '';
				return;
			}
			var model = {
				Server: vm.Server,
				AdminUser: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UserName: vm.UserName,
				Password: vm.Password
			};

			$http.post('./CheckAppLogin', model, tokenHeaderService.getHeaders())
				.then(function(response) {
					vm.AppLoginOk = response.data.Success,
						vm.AppLoginMessage = response.data.Message;
				});
		};

		vm.CheckAppDb = function() {
			$http.post('./DbExists',
					{
						Server: vm.Server,
						AdminUser: vm.CreateDbUser,
						AdminPassword: vm.CreateDbPassword,
						UserName: vm.UserName,
						Password: vm.Password,
						Database: vm.AppDatabase,
						DbType: 1
					},
					tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.AppDbOk = response.data.Exists;
					vm.AppDbCheckMessage = response.data.Message;
					if (vm.AppDbOk) {
						vm.CheckVersions();
						//vm.CheckUsers();
					}
				}).catch(function(xhr, ajaxOptions, thrownError) {
					vm.AppDbCheckMessage = xhr.Message + xhr.ExceptionMessage;
				});
		};

		vm.CheckAnalDb = function() {
			$http.post('./DbExists',
					{
						Server: vm.Server,
						AdminUser: vm.CreateDbUser,
						AdminPassword: vm.CreateDbPassword,
						UserName: vm.UserName,
						Password: vm.Password,
						Database: vm.AnalyticsDatabase,
						DbType: 2
					},
					tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.AnalDbOk = response.data.Exists;
					vm.AnalDbCheckMessage = response.data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					vm.AnalDbCheckMessage = xhr.Message + xhr.ExceptionMessage;
				});
		};

		vm.CheckAggDb = function() {
			if (vm.AggDatabase === "") {
				vm.AggDbOk = false;
				vm.AggDbCheckMessage = "Input Aggregation database (this is optional)";
				return;
			}
			$http.post('./DbExists',
					{
						Server: vm.Server,
						AdminUser: vm.CreateDbUser,
						AdminPassword: vm.CreateDbPassword,
						UserName: vm.UserName,
						Password: vm.Password,
						Database: vm.AggDatabase,
						DbType: 3
					},
					tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.AggDbOk = response.data.Exists;
					vm.AggDbCheckMessage = response.data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					vm.AggDbCheckMessage = xhr.Message + xhr.ExceptionMessage;
				});
		};

		vm.CheckVersions = function() {
			vm.ToEarlyVersionMessage = '';
			$http.post('./api/UpgradeDatabases/GetVersions',
					{
						Server: vm.Server,
						UserName: vm.UserName,
						Password: vm.Password,
						AppDatabase: vm.AppDatabase
					},
					tokenHeaderService.getHeaders())
				.then(function(response) {
					vm.HeadVersion = response.data.HeadVersion;
					vm.ImportAppVersion = response.data.ImportAppVersion;
					vm.AppVersionOk = response.data.AppVersionOk;
					vm.ToEarly = !(response.data.ImportAppVersion > 360);
					if (vm.ToEarly) {
						vm.ToEarlyVersionMessage = 'This is a version that is too early to import this way!';
					}
				});
		};

		vm.startImport = function () {
			if (vm.AppVersionOk !== true && vm.SqlUserOk !== true) {
				alert("When importing an older version you must provide a valid account to upgrade the databases.");
				return;
			}
			// hide button
			vm.ToEarly = true;
			$("#loading").show();
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
				.then(function (response) {
					vm.Success = response.data.Success;
					vm.Message = response.data.Message;
					vm.TenantId = response.data.TenantId;
					$("#loading").hide();
					vm.GetImportLog();
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
					$("#loading").hide();
				});
		};

		vm.GetImportLog = function() {
			$http.post('./GetImportLog', vm.TenantId, tokenHeaderService.getHeaders())
				.then(function(response) {
					vm.Log = response.data;
				});
		};
	}
})();