(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('changePasswordController', changePasswordController);

	function changePasswordController($http, $routeParams) {
		var vm = this;
		vm.UserId = $routeParams.id;
		vm.OldPassword = "";
		vm.Password = "";
		vm.ConfirmPassword = "";

		vm.OldOk = false;
		vm.OldPasswordMessage = "The old password can not be empty";
		vm.PasswordMessage = "The password can not be empty";

		vm.ErrorMessage = "";

		vm.CheckOld = function() {
			if (vm.Name === '') {
				vm.OldPasswordMessage = "The old password can not be empty";
				vm.OldOk = false;
				return;
			}
			vm.OldPasswordMessage = "";
			vm.OldOk = true;
		};

		vm.CheckPassword = function() {
			if (vm.Password === '') {
				vm.PasswordMessage = "The password can not be empty";
				vm.PasswordOk = false;
				return;
			}
			if (vm.Password !== vm.ConfirmPassword) {
				vm.PasswordMessage = "The passwords are not matching.";
				vm.PasswordOk = false;
				return;
			}
			vm.PasswordMessage = "";
			vm.PasswordOk = true;
		};

		vm.save = function () {
			$http.post('./ChangePassword', {
				Id: vm.UserId,
				OldPassword: vm.OldPassword,
				NewPassword: vm.Password,
				ConfirmNewPassword: vm.ConfirmPassword
				})
				.then(function (response) {
					if (!response.data.Success) {
						vm.ErrorMessage = response.data.Message;
						return;
					}
					window.location = "#";
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};
	}
})();