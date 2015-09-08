(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('adduserController', adduserController, ['tokenHeaderService']);

	function adduserController($http, tokenHeaderService) {
		var vm = this;
		
		vm.Name = "";
		vm.Email = "";
		vm.Password = "";
		vm.ConfirmPassword = "";

		vm.NameOk = false;
		vm.NameMessage = "The name can not be empty";
		vm.EmailMessage = "The email does not look to be correct";
		vm.PasswordMessage = "The password can not be empty";

		vm.ErrorMessage = "";
		
		vm.CheckName = function () {
			if(vm.Name === '')
			{
				vm.NameMessage = "The name can not be empty";
				vm.NameOk = false;
				return;
			}
			vm.NameMessage = "Name ok";
			vm.NameOk = true;
		}

		vm.CheckEmail = function () {
			if (vm.Email === undefined) {
				vm.EmailMessage = "The email does not look to be correct";
				vm.EmailOk = false;
				return;
			}
			vm.EmailMessage = "Email ok";
			vm.EmailOk = true;
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
			vm.PasswordMessage = "Password ok";
			vm.PasswordOk = true;
		}


		vm.save = function () {
			$http.post('./AddUser', {
				Name: vm.Name,
				Email: vm.Email,
				Password: vm.Password,
				ConfirmPassword: vm.ConfirmPassword
			}, tokenHeaderService.getHeaders())
				.success(function (data) {
					if (!data.Success) {
						vm.ErrorMessage = data.Message;
						return;
					}
					window.location = "#users";
				})
				.error(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
	}

})();