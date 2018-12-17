(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('detailsController', ['$http', '$routeParams', '$mdDialog', detailsController]);

	function detailsController($http, $routeParams, $mdDialog) {
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
			$http.post('./GetOneTenant', '"' + vm.Tenant + '"')
				.then(function(response) {
					vm.TenantMessage = response.data.Message;
					vm.TenantId = response.data.Id,
						vm.TenantOk = response.data.Success;
					vm.Server = response.data.Server;
					vm.UseIntegratedSecurity = response.data.UseIntegratedSecurity;
					vm.AnalyticsDatabase = response.data.AnalyticsDatabase;
					vm.AppDatabase = response.data.AppDatabase;
					vm.AggregationDatabase = response.data.AggregationDatabase;
					vm.Version = response.data.Version;
					vm.CommandTimeout = response.data.CommandTimeout;
					vm.Active = response.data.Active;
					vm.MobileQRCodeUrl = response.data.MobileQRCodeUrl;
					vm.MaximumSessionTime = response.data.MaximumSessionTime === 0
						? undefined
						: response.data.MaximumSessionTime;
					//vm.CheckAppDb();
					//vm.CheckAnalDb();
					vm.CheckDelete();
					vm.GetImportLog();
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

			$http.post('./CheckImportAdmin', model)
				.then(function(response) {
					vm.SqlUserOk = response.data.Success,
						vm.SqlUserOkMessage = response.data.Message;
				});
		};

		vm.CheckDelete = function() {
			$http.post('./TenantCanBeDeleted', '"' + vm.OriginalName + '"')
				.then(function(response) {
					vm.AllowDelete = response.data.Success;
				});
		};

		vm.Delete = function() {
			$http.post('./DeleteTenant', '"' + vm.OriginalName + '"')
				.then(function(response) {
					if (response.data.Success === false) {
						vm.Message = response.data.Message;
						return;
					}
					window.location = "#";
				});
		};

		vm.UpgradeTenant = function() {
			var model = {
				Tenant: vm.OriginalName,
				AdminUserName: vm.CreateDbUser,
				AdminPassword: vm.CreateDbPassword,
				UseIntegratedSecurity: vm.UseIntegratedSecurity
			};
			$http.post('./UpgradeTenant', model)
				.then(function(response) {
					if (response.data.Success === false) {
						vm.Message = response.data.Message;
						return;
					}
					window.location = "#";
				});
		};

		vm.AddBu = function() {
			var model = {
				Tenant: vm.OriginalName,
				BuName: vm.BuName

			};
			$http.post('./AddBusinessUnitToTenant', model)
				.then(function(response) {
					vm.NewBusinessUnitMessage = response.data.Message;
				});
		};
		vm.LoadTenant();

		

		vm.save = function() {
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
					})
				.then(function (response) {
					if (response.data.Success === false) {
						vm.Message = response.data.Message;
						return;
					}
					window.location = "#";
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.status + xhr.responseText + thrownError;
					vm.Success = false;
				});
		};

		vm.GetImportLog = function() {
			$http.post('./GetImportLog', vm.TenantId)
				.then(function(response) {
					vm.Log = response.data;
				});
		};
	}
})();