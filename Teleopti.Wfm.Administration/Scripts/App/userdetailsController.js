(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('userdetailsController', userdetailsController, []);

	function userdetailsController($http, $routeParams) {
		var vm = this;
		var tokenKey = 'accessToken';
		vm.UserId = $routeParams.id;
		vm.Name = "";
		vm.OriginalName = "";
		vm.Email = "";

		vm.NameOk = false;
		vm.NameMessage = "The name can not be empty";

		vm.token = sessionStorage.getItem(tokenKey);
		if (vm.token === null) {
			return;
		}

		function getHeaders() {
			return {
				headers: { 'Authorization': 'Bearer ' + vm.token }
			};
		}

		vm.LoadUser = function () {
			$http.post('./User', vm.UserId, getHeaders())
				.success(function (data) {
					vm.UserId = data.Id;
					vm.Name = data.Name;
					vm.OriginalName = data.Name;
					vm.Email = data.Email;
					vm.CheckName();
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

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

		vm.LoadUser();
		vm.save = function () {
			
			
			$http.post('./SaveUser', {
				Id: vm.UserId,
				Name: vm.Name,
				Email: vm.Email
			}, getHeaders())
				.success(function (data) {
					window.location = "#users";
				})
				.error(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.status + xhr.responseText + thrownError;
					vm.Success = false;
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		};
	}

})();