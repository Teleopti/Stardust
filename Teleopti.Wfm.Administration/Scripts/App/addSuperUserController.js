(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('addSuperUserController', addSuperUserController);

	function addSuperUserController($http, $routeParams) {
		var vm = this;
		vm.Tenant = $routeParams.tenant;
		vm.FirstName = "";
		vm.LastName = "";
		vm.UserName = "";
		vm.Password = "";
		vm.ConfirmPassword = "";
		vm.Message = "";

		vm.NewUserOk = false;
		vm.NewUserOkMessage = "All fields must be filled in.";

		vm.CheckUser = function () {
			vm.NewUserOkMessage = "All fields must be filled in.";
			if (vm.FirstName === '' || vm.LastName === '') {
				vm.NewUserOk = false;
				return;
			}
			if (vm.UserName === '' || vm.Password === '') {
				vm.NewUserOk = false;
				return;
			}
			if (vm.Password !== vm.ConfirmPassword) {
				vm.NewUserOk = false;
				vm.NewUserOkMessage = "Password does not match the confirm password.";
				return;
			}
			var model = {
				FirstUser: vm.UserName,
				FirstUserPassword: vm.Password
			};

			$http.post('./CheckFirstUser', model)
				.then(function(response) {
					vm.NewUserOk = response.data.Success,
						vm.NewUserOkMessage = response.data.Message;
				});
		};

		vm.save = function () {
			$http.post('./AddSuperUserToTenant', {
				Tenant: vm.Tenant,
				FirstName: vm.FirstName,
				LastName: vm.LastName,
				UserName: vm.UserName,
				Password: vm.Password
			})
				.then(function (response) {
					if (response.data.Success === false) {
						vm.Message = response.data.Message;
						return;
					}
					window.location = "#";
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};

		vm.ShowHide = function () {
			if (vm.Password !== "")
				$(".glyphicon-eye-open").show();
			else
				$(".glyphicon-eye-open").hide();
		}

		$("#passwordfield").on("keyup", function () {
			if ($(this).val())
				$(".glyphicon-eye-open").show();
			else
				$(".glyphicon-eye-open").hide();
		});

		$(".glyphicon-eye-open").mousedown(function () {
			$("#passwordfield").attr('type', 'text');
		}).mouseup(function () {
			$("#passwordfield").attr('type', 'password');
		}).mouseout(function () {
			$("#passwordfield").attr('type', 'password');
			});

	}

	
})();
