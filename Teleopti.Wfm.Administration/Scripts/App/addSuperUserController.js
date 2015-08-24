﻿(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('addSuperUserController', addSuperUserController, []);

	function addSuperUserController($http, $routeParams) {
		var vm = this;
		var tokenKey = 'accessToken';
		vm.Tenant = $routeParams.tenant;
		vm.FirstName = "";
		vm.LastName = "";
		vm.UserName = "";
		vm.Password = "";
		vm.Message = "";

		vm.NewUserOk = false;
		vm.NewUserOkMessage = "";

		vm.token = sessionStorage.getItem(tokenKey);
		if (vm.token === null) {
			return;
		}

		function getHeaders() {
			return {
				headers: { 'Authorization': 'Bearer ' + vm.token }
			};
		}

		vm.CheckUser = function () {
			var model = {
				FirstUser: vm.UserName,
				FirstUserPassword: vm.Password
			}

			$http.post('./CheckFirstUser', model, getHeaders())
			.success(function (data) {
				vm.NewUserOk = data.Success,
				vm.NewUserOkMessage = data.Message;

			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.status + xhr.responseText + thrownError);
			});
		}

		vm.save = function () {
			$http.post('./AddSuperUserToTenant', {
				Tenant: vm.Tenant,
				FirstName: vm.FirstName,
				LastName: vm.LastName,
				UserName: vm.UserName,
				Password: vm.Password
			}, getHeaders())
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
