(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('createController', createController, []);

	function createController($http) {
		var vm = this;
		$("#loading").hide();
		var tokenKey = 'accessToken';

		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;

		vm.Tenant = '';
		vm.CreateDbUser = '';
		vm.CreateDbPassword = '';
		vm.SqlUserOkMessage = '';
		vm.SqlUserOk = false;
		vm.AppUser = '';
		vm.AppPassword = '';
		vm.FirstUser = '';
		vm.FirstUserPassword = '';
		vm.BusinessUnit = '';
		vm.Success = false;
		vm.Message = '';
		vm.Creating = '';
		vm.FirstUserOk = false;
		vm.FirstUserOkMessage = '';

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
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.CheckServer = function () {
			var model = {
				CreateDbUser: vm.CreateDbUser,
				CreateDbPassword: vm.CreateDbPassword
			}

			$http.post('./CheckCreateDb', model, getHeaders())
			.success(function (data) {
				vm.SqlUserOk = data.Success,
				vm.SqlUserOkMessage = data.Message;

			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.status + xhr.responseText + thrownError);
			});
		}
		vm.Create = function () {
			vm.Creating = 'Calling server to createe new Tenant...';
			vm.Message = '';
			$("#loading").show();

			var model = {
				Tenant: vm.Tenant,
				Server: vm.Server,
				CreateDbUser: vm.CreateDbUser,
				CreateDbPassword: vm.CreateDbPassword,
				AppUser: vm.AppUser,
				AppPassword: vm.AppPassword,
				FirstUser: vm.FirstUser,
				FirstUserPassword: vm.FirstUserPassword,
				BusinessUnit: vm.BusinessUnit
			}
			
			$http.post('./CreateTenant', model, getHeaders())
			.success(function (data) {
				vm.Success = data.Success,
				vm.Message = data.Message;
				vm.Creating = '';
				$("#loading").hide();

			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.status + xhr.responseText + thrownError);
				$("#loading").hide();
			});
		}
		vm.CheckFirstUser = function () {
			var model = {
				FirstUser: vm.FirstUser,
				FirstUserPassword: vm.FirstUserPassword
			}

			$http.post('./CheckFirstUser', model, getHeaders())
			.success(function (data) {
				vm.FirstUserOk = data.Success,
				vm.FirstUserOkMessage = data.Message;

			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.status + xhr.responseText + thrownError);
			});
		}
	}

})();