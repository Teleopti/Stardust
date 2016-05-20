(function ($http) {
	'use strict';
	var tokenKey = 'accessToken';
	var userKey = 'userToken';
	var emailKey = 'lastEmail';
	var idKey = 'idToken';
	var firstUser = false;

	var token = sessionStorage.getItem(tokenKey);
    
	angular
		 .module('adminApp')
		 .controller('loginController', loginController, []);

	function loginController($scope, $http) {
		var vm = this;
		vm.loginEmail = sessionStorage.getItem(emailKey);
		vm.loginPassword = "";
		vm.Message = '';
		vm.Id = sessionStorage.getItem(idKey);

		vm.user = sessionStorage.getItem(userKey);
		$scope.selected = 1;

		$http.get("./HasNoUser").success(function (data) {
		    firstUser = data;
		    if (firstUser) {
		        vm.user = 'xxfirstxx';
		        window.location = "#adduser";
		    } else {
		        if (token === null) {
		            $("#modal-login").dialog({
		                modal: true,
		                title: "Log in to access the admin site"
		            }
                    );
		        }
		    }
		}).error(function (xhr, ajaxOptions, thrownError) {
		    console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

		function showError(jqXHR) {
			vm.Message = jqXHR.Message + ': ' + jqXHR.ExceptionMessage;
		}

		vm.login = function () {
			vm.Id = 0;
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
				vm.Id = data.Id;
				vm.Message = 'Successful log in...';
				// Cache the access token in session storage.
				sessionStorage.setItem(tokenKey, data.AccessToken);
				sessionStorage.setItem(userKey, data.UserName);
				sessionStorage.setItem(emailKey, vm.loginEmail);
				sessionStorage.setItem(idKey, data.Id);
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