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
				.then(function(data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
					//vm.CheckUsers();

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.SqlUserOk = data.Success,
						vm.SqlUserOkMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.AppLoginOk = data.Success,
						vm.AppLoginMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.AppDbOk = data.Exists;
					vm.AppDbCheckMessage = data.Message;
					if (vm.AppDbOk) {
						vm.CheckVersions();
						//vm.CheckUsers();
					}
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.AnalDbOk = data.Exists;
					vm.AnalDbCheckMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.AggDbOk = data.Exists;
					vm.AggDbCheckMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
				.then(function(data) {
					vm.HeadVersion = data.HeadVersion;
					vm.ImportAppVersion = data.ImportAppVersion;
					vm.AppVersionOk = data.AppVersionOk;
					vm.ToEarly = !(data.ImportAppVersion > 360);
					if (vm.ToEarly) {
						vm.ToEarlyVersionMessage = 'This is a version that is too early to import this way!';
					}

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
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
				.then(function (data) {
					vm.Success = data.Success;
					vm.Message = data.Message;
					vm.TenantId = data.TenantId;
					$("#loading").hide();
					vm.GetImportLog();
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
					$("#loading").hide();
				});
		};

		vm.GetImportLog = function() {
			$http.post('./GetImportLog', vm.TenantId, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.Log = data;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
	}

})();