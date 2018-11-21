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
		vm.EmailOk = false;
		vm.PasswordOk = false;
		vm.NameMessage = "The name can not be empty.";
		vm.EmailMessage = "The email does not look to be correct.";
		vm.PasswordMessage = "The password can not be empty.";

		vm.ErrorMessage = "";
		vm.FirstUser = false;
		vm.SaveEnabled = false;

		$http.get("./HasNoUser").success(function(data) {
			vm.FirstUser = data;
		}).error(function(xhr, ajaxOptions, thrownError) {
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
						},
						tokenHeaderService.getHeaders())
					.success(function(data) {
						if (!data.Success) {
							vm.EmailMessage = data.Message;
							vm.EmailOk = false;
						} else {
							vm.EmailMessage = "Email ok.";
							vm.EmailOk = true;
						}

					})
					.error(function(xhr, ajaxOptions, thrownError) {
						vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
						vm.Success = false;
						console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
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
					},
					tokenHeaderService.getHeaders())
				.success(function(data) {
					if (!data.Success) {
						vm.ErrorMessage = data.Message;
						return;
					}
					window.location = "#users";
				})
				.error(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.savefirst = function() {
			$http.post('./AddFirstUser',
					{
						Name: vm.Name,
						Email: vm.Email,
						Password: vm.Password,
						ConfirmPassword: vm.ConfirmPassword
					}).success(function(data) {
					if (!data.Success) {
						vm.ErrorMessage = data.Message;
						return;
					}
					window.location = document.location.toString().replace("firstuser.html", "");
				})
				.error(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
	}

})();