(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('userdetailsController', userdetailsController);

	function userdetailsController($http, $routeParams, $cookies) {
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
			$http.post('./User', vm.UserId)
				.then(function (response) {
					vm.UserId = response.data.Id;
					vm.Name = response.data.Name;
					vm.OriginalName = response.data.Name;
					vm.Email = response.data.Email;
					vm.CheckName();
					vm.CheckEmail();
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

		vm.LoadUser();
		vm.save = function() {
			var cookie = $cookies.getObject('WfmAdminAuth');

			var loggedOnId = cookie ? cookie.id : 0;

			$http.post('./SaveUser',
					{
						Id: vm.UserId,
						Name: vm.Name,
						Email: vm.Email
					})
				.then(function (response) {
					if (!response.data.Success) {
						vm.Message = response.data.Message;
						return;
					}
					if (vm.UserId === loggedOnId) {
						cookie.user = vm.Name;
						var today = new Date();
						var newExpireDate = new Date(today.getTime() + 30 * 60000);
						$cookies.putObject('WfmAdminAuth', cookie, { 'expires': newExpireDate });
						window.location = "/#";
						window.location.reload();
						return;
					}
					window.location = "#users";
				})
				.catch(function(xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};

		vm.DeleteUser = function() {
			$http.post('./DeleteUser', vm.UserId)
				.then(function (response) {
					window.location = "#users";
				});
		};
	}

})();