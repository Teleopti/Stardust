(function () {
	'use strict';
	var tokenKey = 'accessToken';
	var userKey = 'userToken';
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
		vm.loginEmail = "";
		vm.loginPassword = "";
		vm.user = sessionStorage.getItem(userKey);

		function showError(jqXHR) {
			vm.result = jqXHR.status + ': ' + jqXHR.statusText;
		}

		vm.login = function () {
			vm.result = '';

			var loginData = {
				granttype: 'password',
				username: vm.loginEmail,
				password: vm.loginPassword
			};

			$.ajax({
				type: 'POST',
				url: '/Login',
				data: loginData
			}).done(function (data) {
				vm.user = data.userName;
				// Cache the access token in session storage.
				sessionStorage.setItem(tokenKey, data.AccessToken);
				sessionStorage.setItem(userKey, data.UserName);
				document.location = "#";
				$('#modal-login').dialog('close');
			}).fail(showError);
		}

		vm.logout = function () {
			sessionStorage.removeItem(tokenKey);
			sessionStorage.removeItem(userKey);
			vm.user = null;
		}

		
	}

})();