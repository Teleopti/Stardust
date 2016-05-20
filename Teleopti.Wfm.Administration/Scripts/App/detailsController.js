﻿(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('detailsController', detailsController, ['tokenHeaderService']);

	function detailsController($http, $routeParams, tokenHeaderService) {
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

		vm.LoadTenant = function () {
			$http.post('./GetOneTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.TenantMessage = data.Message;
					vm.TenantId = data.Id,
					vm.TenantOk = data.Success;
					vm.Server = data.Server;
					vm.UserName = data.UserName;
					vm.Password = data.Password;
					vm.AnalyticsDatabase = data.AnalyticsDatabase;
					vm.AppDatabase = data.AppDatabase;
					vm.AggregationDatabase = data.AggregationDatabase;
					vm.Version = data.Version;
					vm.CommandTimeout = data.CommandTimeout;
			        vm.Active = data.Active;
					//vm.CheckAppDb();
					//vm.CheckAnalDb();
					vm.CheckDelete();
					vm.GetImportLog();
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		vm.ShowUpgrade = function () {
			return true;
			if (vm.Version === null) return false;
			return vm.Version.AppVersionOk === false;
		};

		vm.ShowTheLog = false;
		vm.ShowLog = false;
		vm.ShowHide = function () {
			//If DIV is visible it will be hidden and vice versa.
			vm.ShowTheLog = vm.ShowLog;
		}

		vm.CheckImportAdmin = function () {
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
			}

			$http.post('./CheckImportAdmin', model, tokenHeaderService.getHeaders())
			.success(function (data) {
				vm.SqlUserOk = data.Success,
				vm.SqlUserOkMessage = data.Message;
			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});
		}
		//vm.CheckTenantName = function () {
		//	$http.post('./api/Home/NameIsFree', {
		//		OriginalName: vm.OriginalName,
		//		NewName: vm.Tenant,
		//		AppDatabase: vm.AppDatabase,
		//		AnalyticsDatabase: vm.AnalyticsDatabase
		//	}, tokenHeaderService.getHeaders())
		//		.success(function (data) {
		//			vm.TenantMessage = data.Message;
		//			vm.TenantOk = data.Success;
		//		}).error(function (xhr, ajaxOptions, thrownError) {
		//			console.log(xhr.status + xhr.responseText + thrownError);
		//		});
		//}

		vm.CheckDelete = function () {
			$http.post('./TenantCanBeDeleted', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.AllowDelete = data.Success;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		vm.Delete = function() {
			$http.post('./DeleteTenant', '"' + vm.OriginalName + '"', tokenHeaderService.getHeaders())
				.success(function(data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				}).error(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		vm.UpgradeTenant = function () {
			var model = {
				Tenant: vm.OriginalName,
				AdminUserName: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UseIntegratedSecurity: vm.UseIntegratedSecurity
			}
			$http.post('./UpgradeTenant', model, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (data.Success === false) {
						vm.Message = data.Message;
						return;
					}
					window.location = "#";
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		vm.AddBu = function () {
			var model = {
				Tenant: vm.OriginalName,
				BuName: vm.BuName

			}
			$http.post('./AddBusinessUnitToTenant', model, tokenHeaderService.getHeaders())
				.success(function (data) {
						vm.NewBusinessUnitMessage = data.Message;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}
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
		//		.success(function (data) {
		//			vm.AppDbOk = data.Exists;
		//			vm.AppDbCheckMessage = data.Message;
		//			//display how many maybe
		//			//if (vm.AppDbOk) {
		//			//	vm.CheckUsers();
		//			//}
		//		}).error(function (xhr, ajaxOptions, thrownError) {
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
		//		.success(function (data) {
		//			vm.AnalDbOk = data.Exists;
		//			vm.AnalDbCheckMessage = data.Message;
		//		}).error(function (xhr, ajaxOptions, thrownError) {
		//			console.log(xhr.status + xhr.responseText + thrownError);
		//		});
		//}
		
		vm.save = function () {
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

					$http.post('./UpdateTenant', {
						OriginalName: vm.OriginalName,
						NewName: vm.Tenant,
						AppDatabase: vm.AppDatabase,
						AnalyticsDatabase: vm.AnalyticsDatabase,
						Server: vm.Server,
						UserName: vm.UserName,
						Password: vm.Password,
						CommandTimeout: vm.CommandTimeout,
                        Active : vm.Active
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

			}

		vm.GetImportLog = function () {
			$http.post('./GetImportLog', vm.TenantId, tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.Log = data;
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		}

		}

})();