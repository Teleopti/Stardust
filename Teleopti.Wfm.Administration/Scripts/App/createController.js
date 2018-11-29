(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('createController', createController, ['tokenHeaderService']);

	function createController($http, tokenHeaderService) {
		var vm = this;
		$("#loading").hide();

		vm.TenantMessage = "Enter a new name for the Tenant";
		vm.TenantOk = false;

		vm.Tenant = '';
		vm.CreateDbUser = '';
		vm.CreateDbPassword = '';
		vm.SqlUserOkMessage = '';
		vm.SqlUserOk = false;

		vm.AppUser = '';
		vm.AppPassword = '';
		vm.UserOkMessage = '';
		vm.UserOk = false;

		vm.BusinessUnit = '';
		vm.BusinessUnitOk = false;
		vm.BusinessUnitOkMessage = 'The Business Unit can not be empty.';

		vm.FirstUser = '';
		vm.FirstUserPassword = '';
		vm.FirstUserOk = false;
		vm.FirstUserOkMessage = 'The user and password can not be empty.';

		vm.Success = false;
		vm.Message = '';
		vm.Creating = '';

		vm.BuAndUserOk = function() {
			return vm.FirstUserOk === true && vm.BusinessUnitOk === true;
		};

		vm.CheckBU = function() {
			vm.Message = '';
			if (vm.BusinessUnit !== '') {
				vm.BusinessUnitOk = true;
				vm.BusinessUnitOkMessage = 'Name of Business Unit is ok.';
				return;
			}
			vm.BusinessUnitOk = false;
			vm.BusinessUnitOkMessage = 'The Business Unit can not be empty.';
		};

		vm.CheckTenantName = function() {
			vm.Message = '';
			$http.post('./api/Import/IsNewTenant', '"' + vm.Tenant + '"', tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.TenantMessage = data.Message;
					vm.TenantOk = data.Success;
				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.CheckServer = function() {
			vm.Message = '';
			if (vm.CreateDbUser === '' || vm.CreateDbPassword === '') {
				vm.SqlUserOkMessage = '';
				return;
			}
			var model = {
				CreateDbUser: vm.CreateDbUser,
				CreateDbPassword: vm.CreateDbPassword
			};

			$http.post('./CheckCreateDb', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.SqlUserOk = data.Success,
						vm.SqlUserOkMessage = data.Message;
					vm.CheckLogin();

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.CheckLogin = function() {
			vm.Message = '';
			if (vm.AppUser === '' || vm.AppPassword === '') {
				vm.UserOkMessage = 'Fill in both login and password.';
				vm.UserOk = false;
				return;
			}
			if (vm.SqlUserOk !== true) {
				vm.UserOkMessage = 'Can not check login at the moment.';
				vm.UserOk = false;
				return;
			}
			var model = {
				CreateDbUser: vm.CreateDbUser,
				CreateDbPassword: vm.CreateDbPassword,
				AppUser: vm.AppUser,
				AppPassword: vm.AppPassword
			};

			$http.post('./CheckLogin', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.UserOk = data.Success,
						vm.UserOkMessage = data.Message;

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.Create = function() {
			vm.Creating = 'Calling server to create new Tenant...';
			vm.Message = '';
			$("#loading").show();

			var model = {
				Tenant: vm.Tenant,
				CreateDbUser: vm.CreateDbUser,
				CreateDbPassword: vm.CreateDbPassword,
				AppUser: vm.AppUser,
				AppPassword: vm.AppPassword,
				FirstUser: vm.FirstUser,
				FirstUserPassword: vm.FirstUserPassword,
				BusinessUnit: vm.BusinessUnit
			};

			$http.post('./CreateTenant', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.Success = data.Success,
						vm.Message = data.Message;
					vm.Creating = '';
					$("#loading").hide();

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Creating = '';
					vm.Success = false;
					$("#loading").hide();
				});
		};

		vm.CheckFirstUser = function() {
			var model = {
				FirstUser: vm.FirstUser,
				FirstUserPassword: vm.FirstUserPassword
			};

			$http.post('./CheckFirstUser', model, tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.FirstUserOk = data.Success,
						vm.FirstUserOkMessage = data.Message;

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
	}

})();