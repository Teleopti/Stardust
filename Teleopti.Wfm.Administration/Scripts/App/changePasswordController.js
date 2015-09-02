(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('changePasswordController', changePasswordController, ['tokenHeaderService']);

	function changePasswordController($http, $routeParams, tokenHeaderService) {
		var vm = this;;
		vm.UserId = $routeParams.id;
		vm.OldPassword = "";
		vm.Password = "";
		vm.ConfirmPassword = "";

		vm.OldOk = false;
		vm.OldPasswordMessage = "The old password can not be empty";
		vm.PasswordMessage = "The password can not be empty";

		vm.ErrorMessage = "";
		
		vm.CheckOld = function () {
			if (vm.Name === '') {
				vm.OldPasswordMessage = "The old password can not be empty";
				vm.OldOk = false;
				return;
			}
			vm.OldPasswordMessage = "";
			vm.OldOk = true;
		}

		vm.CheckPassword = function () {
			if (vm.Password === '') {
				vm.PasswordMessage = "The password can not be empty";
				vm.PasswordOk = false;
				return;
			}
			if (vm.Password !== vm.ConfirmPassword) {
				vm.PasswordMessage = "The passwords do no match";
				vm.PasswordOk = false;
				return;
			}
			vm.PasswordMessage = "";
			vm.PasswordOk = true;
		}


		vm.save = function () {
			$http.post('./ChangePassword', {
				Id: vm.UserId,
				OldPassword: vm.OldPassword,
				NewPassword: vm.Password,
				ConfirmNewPassword: vm.ConfirmPassword
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (!data.Success) {
						vm.ErrorMessage = data.Message;
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