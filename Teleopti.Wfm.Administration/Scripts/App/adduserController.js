(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('adduserController', adduserController);

	function adduserController($http) {
		var vm = this;
		
		vm.Name = "";
		vm.Email = "";
		vm.Password = "";
		vm.ConfirmPassword = "";

		vm.NameOk = false;
		vm.EmailOk = false;
		vm.PasswordOk = false;
		vm.NameMessage = "The name can not be empty.";
		vm.EmailMessage = "The email does not look to be correct.";
		vm.PasswordMessage = "The password can not be empty.";

		vm.ErrorMessage = "";
		vm.FirstUser = false;
		vm.SaveEnabled = false;

		$http.get("./HasNoUser").then(function (response) {
			vm.FirstUser = response.data;
		}).catch(function(xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

		vm.CheckAll = function() {
			if (vm.NameOk && vm.EmailOk && vm.PasswordOk) {
				vm.SaveEnabled = true;
			} else {
				vm.SaveEnabled = false;
			}
		};

		vm.CheckName = function() {
			if (vm.Name === "") {
				vm.NameMessage = "The name can not be empty.";
				vm.NameOk = false;
			} else {
				vm.NameMessage = "Name ok.";
				vm.NameOk = true;
			}
		};

		vm.CheckEmail = function() {
			if (vm.Email === undefined || vm.Email === "") {
				vm.EmailMessage = "The email does not look to be correct.";
				vm.EmailOk = false;
			} else {
				vm.EmailMessage = "Email ok.";
				vm.EmailOk = true;
			}
			if (vm.EmailOk) {
				$http.post("./CheckEmail",
						{
							Email: vm.Email
						})
					.then(function (response) {
						if (!response.data.Success) {
							vm.EmailMessage = response.data.Message;
							vm.EmailOk = false;
						} else {
							vm.EmailMessage = "Email ok.";
							vm.EmailOk = true;
						}

					})
					.catch(function(xhr, ajaxOptions, thrownError) {
						vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
						vm.Success = false;
					});
			}
			vm.CheckAll();
		};

		vm.CheckPassword = function() {
			if (vm.Password === '') {
				vm.PasswordMessage = "The password can not be empty.";
				vm.PasswordOk = false;
			} else if (vm.Password.length < 6) {
				vm.PasswordMessage = "The must be at least 6 characters.";
				vm.PasswordOk = false;
			} else if (vm.Password !== vm.ConfirmPassword) {
				vm.PasswordMessage = "The passwords are not matching.";
				vm.PasswordOk = false;
			} else {
				vm.PasswordMessage = "Password ok.";
				vm.PasswordOk = true;
			}
			vm.CheckAll();
		};

		vm.save = function() {
			if (vm.FirstUser === true) {
				vm.savefirst();
				return;
			}
			vm.addUser();
		};

		vm.addUser = function() {
			$http.post('./AddUser',
					{
						Name: vm.Name,
						Email: vm.Email,
						Password: vm.Password,
						ConfirmPassword: vm.ConfirmPassword
					})
				.then(function (response) {
					if (!response.data.Success) {
						vm.ErrorMessage = response.data.Message;
						return;
					}
					window.location = "#users";
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};

		vm.savefirst = function() {
			$http.post('./AddFirstUser',
					{
						Name: vm.Name,
						Email: vm.Email,
						Password: vm.Password,
						ConfirmPassword: vm.ConfirmPassword
				}).then(function (response) {
					if (!response.data.Success) {
						vm.ErrorMessage = response.data.Message;
						return;
					}
					window.location = document.location.toString().replace("firstuser.html", "");
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};
	}
})();