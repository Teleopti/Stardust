(function () {
	'use strict';

	angular
		 .module('adminApp').controller('detailsController',
		 ['$http', '$routeParams', 'tokenHeaderService', '$mdDialog', detailsController]);

	function detailsController($http, $routeParams, tokenHeaderService, $mdDialog) {
		var vm = this;

		vm.Tenant = $routeParams.tenant;
		vm.OriginalName = $routeParams.tenant;
		vm.TenantId = -1;
		vm.Server = "";
		vm.UserName = "";
		vm.Password = "";
		vm.AppDatabase = "";
		vm.AnalyticsDatabase = "";
		vm.AggregationDatabase = "";
		vm.Version = null;
	    vm.Active = true;
		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;
		vm.AppDbOk = false;
		vm.AppDbCheckMessage = "Input Application database";

		vm.AnalDbOk = false;
		vm.AnalDbCheckMessage = "Input Analytics database";
		vm.CommandTimeout = 0;
		vm.MaximumSessionTime = undefined;
		vm.Message = "";
		vm.AllowDelete = false;
		
		vm.CreateDbUser = '';
		vm.CreateDbPassword = '';
		vm.UseIntegratedSecurity = false;
		vm.SqlUserOkMessage = '';
		vm.SqlUserOk = false;

		vm.BuName = '';
		vm.NewBusinessUnitMessage = '';
		vm.NewBuOk = true;
		vm.Log = null;

		vm.showConfirm = function (ev) {
			// Appending dialog to document.body to cover sidenav in docs app
			var confirm = $mdDialog.confirm()
					.title('Would you like to detach this Tenant?')
					.textContent('No database will be removed. You can always import it again if you like.')
					.targetEvent(ev)
					.ok('Please do it!')
					.cancel('No not now.');
					
			$mdDialog.show(confirm).then(function () {
				vm.Delete();
			}, function () {
				//do nada;
			});
		};

		vm.LoadTenant = function() {
			$http.post('./GetOneTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.TenantMessage = data.Message;
					vm.TenantId = data.Id,
						vm.TenantOk = data.Success;
					vm.Server = data.Server;
					vm.UseIntegratedSecurity = data.UseIntegratedSecurity;
					vm.AnalyticsDatabase = data.AnalyticsDatabase;
					vm.AppDatabase = data.AppDatabase;
					vm.AggregationDatabase = data.AggregationDatabase;
					vm.Version = data.Version;
					vm.CommandTimeout = data.CommandTimeout;
					vm.Active = data.Active;
					vm.MobileQRCodeUrl = data.MobileQRCodeUrl;
					vm.MaximumSessionTime = data.MaximumSessionTime === 0 ? undefined : data.MaximumSessionTime;
					//vm.CheckAppDb();
					//vm.CheckAnalDb();
					vm.CheckDelete();
					vm.GetImportLog();
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.ShowUpgrade = function () {
			return true;
			if (vm.Version === null) return false;
			return vm.Version.AppVersionOk === false;
		};

		vm.ShowTheLog = false;
		vm.ShowLog = false;
		vm.ShowHide = function() {
			//If DIV is visible it will be hidden and vice versa.
			vm.ShowTheLog = vm.ShowLog;
		};

		vm.CheckImportAdmin = function() {
			vm.Message = '';
			if (vm.UseIntegratedSecurity && vm.Server === '') {
				vm.SqlUserOkMessage = '';
				vm.SqlUserOk = false;
				return;
			}
			if (vm.UseIntegratedSecurity !== true) {
				if (vm.CreateDbUser === '' || vm.CreateDbPassword === '' || vm.Server === '') {
					vm.SqlUserOkMessage = '';
					vm.SqlUserOk = false;
					return;
				}
			}

			var model = {
				Server: vm.Server,
				AdminUser: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UseIntegratedSecurity: vm.UseIntegratedSecurity
			};

			$http.post('./CheckImportAdmin', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.SqlUserOk = data.Success,
						vm.SqlUserOkMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
		//vm.CheckTenantName = function () {
		//	$http.post('./api/Home/NameIsFree', {
		//		OriginalName: vm.OriginalName,
		//		NewName: vm.Tenant,
		//		AppDatabase: vm.AppDatabase,
		//		AnalyticsDatabase: vm.AnalyticsDatabase
		//	}, tokenHeaderService.getHeaders())
		//		.then(function (data) {
		//			vm.TenantMessage = data.Message;
		//			vm.TenantOk = data.Success;
		//		}).catch(function (xhr, ajaxOptions, thrownError) {
		//			console.log(xhr.status + xhr.responseText + thrownError);
		//		});
		//}

		vm.CheckDelete = function() {
			$http.post('./TenantCanBeDeleted', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.AllowDelete = data.Success;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.Delete = function() {
			$http.post('./DeleteTenant', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.then(function(data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.UpgradeTenant = function() {
			var model = {
				Tenant: vm.OriginalName,
				AdminUserName: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UseIntegratedSecurity: vm.UseIntegratedSecurity
			};
			$http.post('./UpgradeTenant', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.AddBu = function() {
			var model = {
				Tenant: vm.OriginalName,
				BuName: vm.BuName

			};
			$http.post('./AddBusinessUnitToTenant', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.NewBusinessUnitMessage = data.Message;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
		vm.LoadTenant();

		//vm.CheckAppDb = function () {
		//	vm.AppDbOk = false;
		//	vm.AppDbCheckMessage = "Checking database.....";
		//	$http.post('./DbExists', {
		//		Server: vm.Server,
		//		UserName: vm.UserName,
		//		Password: vm.Password,
		//		Database: vm.AppDatabase,
		//		DbType: 1
		//	}, tokenHeaderService.getHeaders())
		//		.then(function (data) {
		//			vm.AppDbOk = data.Exists;
		//			vm.AppDbCheckMessage = data.Message;
		//			//display how many maybe
		//			//if (vm.AppDbOk) {
		//			//	vm.CheckUsers();
		//			//}
		//		}).catch(function (xhr, ajaxOptions, thrownError) {
		//			console.log(xhr.status + xhr.responseText + thrownError);
		//		});
		//}
		//vm.CheckAnalDb = function () {
		//	vm.AnalDbOk = false;
		//	vm.AnalDbCheckMessage = "Checking database.....";
		//	$http.post('./DbExists', {
		//		Server: vm.Server,
		//		UserName: vm.UserName,
		//		Password: vm.Password,
		//		Database: vm.AnalyticsDatabase,
		//		DbType: 2
		//	}, tokenHeaderService.getHeaders())
		//		.then(function (data) {
		//			vm.AnalDbOk = data.Exists;
		//			vm.AnalDbCheckMessage = data.Message;
		//		}).catch(function (xhr, ajaxOptions, thrownError) {
		//			console.log(xhr.status + xhr.responseText + thrownError);
		//		});
		//}

		vm.save = function() {
			//if (vm.AppDbOk === false) {
			//	alert("Fix the settings to the Application database.");
			//	return;
			//}
			//if (vm.AnalDbOk === false) {
			//	alert("Fix the settingsto the Analytics database.");
			//	return;
			//}
			//if (vm.TenantOk === false) {
			//	alert("The new name of the Tenant must be changed before saving.");
			//	return;
			//}

			$http.post('./UpdateTenant',
					{
						OriginalName: vm.OriginalName,
						NewName: vm.Tenant,
						AppDatabase: vm.AppDatabase,
						AnalyticsDatabase: vm.AnalyticsDatabase,
						Server: vm.Server,
						UserName: vm.UserName,
						Password: vm.Password,
						UseIntegratedSecurity: vm.UseIntegratedSecurity,
						CommandTimeout: vm.CommandTimeout,
						Active: vm.Active,
						MobileQRCodeUrl: vm.MobileQRCodeUrl,
						MaximumSessionTime: vm.MaximumSessionTime
					},
					tokenHeaderService.getHeaders())
				.then(function(data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.status + xhr.responseText + thrownError;
					vm.Success = false;
					console.log(xhr.status + xhr.responseText + thrownError);
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