﻿(function () {
	'use strict';
	var tokenKey = 'accessToken';
	var userKey = 'userToken';
	var emailKey = 'lastEmail';
	var token = sessionStorage.getItem(tokenKey);
	if (token === null) {
		$("#modal-login").dialog({
			modal: true,
			title: "Log in to access the admin site"
	}
		);
	}
	angular
		 .module('adminApp')
		 .controller('loginController', loginController, []);

	function loginController($scope, $http) {
		var vm = this;
		vm.loginEmail = sessionStorage.getItem(emailKey);
		vm.loginPassword = "";
		vm.Message = '';
		vm.user = sessionStorage.getItem(userKey);

		function showError(jqXHR) {
			vm.Message = jqXHR.status + ': ' + jqXHR.statusText;
		}

		vm.login = function () {
			$("#modal-login").toggleClass("wait");
			var loginData = {
				granttype: 'password',
				username: vm.loginEmail,
				password: vm.loginPassword
			};

			$http.post('./Login',
				 loginData
			).success(function (data) {
				$("#modal-login").toggleClass("wait");
				if (data.Success === false) {
					//alert(data.Message);
					vm.Message = data.Message;
					return;
				}
				vm.user = data.userName;
				vm.Message = 'Successful log in...';
				// Cache the access token in session storage.
				sessionStorage.setItem(tokenKey, data.AccessToken);
				sessionStorage.setItem(userKey, data.UserName);
				sessionStorage.setItem(emailKey, vm.loginEmail);
				document.location = "#";
				$('#modal-login').dialog('close');
			}).error(showError);
			
		}

		vm.logout = function () {
			sessionStorage.removeItem(tokenKey);
			sessionStorage.removeItem(userKey);
			vm.user = null;
		}
	}

})();