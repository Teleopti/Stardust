(function () {
	'use strict';
	var idKey = 'idToken';
	var userKey = 'userToken';
	angular
		.module('adminApp')
		.controller('userdetailsController', userdetailsController, ['tokenHeaderService']);

	function userdetailsController($http, $routeParams, tokenHeaderService) {
		var vm = this;

		vm.UserId = $routeParams.id;
		vm.Name = "";
		vm.OriginalName = "";
		vm.Email = "";
		vm.EmailOk = true;
		vm.NameOk = true;
		vm.SaveEnabled = true;

		vm.NameMessage = "The name can not be empty.";
		vm.EmailMessage = "The email does not look to be correct.";
		vm.Message = "";

		vm.CheckAll = function() {
			if (vm.NameOk && vm.EmailOk) {
				vm.SaveEnabled = true;
			} else {
				vm.SaveEnabled = false;
			}
		};

		vm.LoadUser = function() {
			$http.post('./User', vm.UserId, tokenHeaderService.getHeaders())
				.success(function(data) {
					vm.UserId = data.Id;
					vm.Name = data.Name;
					vm.OriginalName = data.Name;
					vm.Email = data.Email;
					vm.CheckName();
					vm.CheckEmail();
				}).error(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.CheckName = function() {
			if (vm.Name === '') {
				vm.NameMessage = "The name can not be empty.";
				vm.NameOk = false;
			} else {
				vm.NameMessage = "Name ok.";
				vm.NameOk = true;
			}

			vm.CheckAll();
		};

		vm.CheckEmail = function() {
			if (vm.Email === '') {
				vm.EmailMessage = "The email can not be empty.";
				vm.EmailOk = false;
			} else if (vm.Email === undefined) {
				vm.EmailMessage = "The email does not look to be correct.";
				vm.EmailOk = false;

			} else {
				vm.EmailMessage = "Email ok.";
				vm.EmailOk = true;
			}
			if (vm.EmailOk) {
				$http.post('./CheckEmail',
						{
							Email: vm.Email,
							Id: vm.UserId
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

		vm.LoadUser();
		vm.save = function() {

			var loggedOnId = parseInt(sessionStorage.getItem(idKey));

			$http.post('./SaveUser',
					{
						Id: vm.UserId,
						Name: vm.Name,
						Email: vm.Email
					},
					tokenHeaderService.getHeaders())
				.success(function(data) {
					if (!data.Success) {
						vm.Message = data.Message;
						return;
					}
					if (vm.UserId === loggedOnId) {
						sessionStorage.setItem(userKey, vm.Name);
						window.location = "#";
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

		vm.DeleteUser = function() {
			$http.post('./DeleteUser', vm.UserId, tokenHeaderService.getHeaders())
				.success(function(data) {
					window.location = "#users";
				}).error(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};
	}

})();