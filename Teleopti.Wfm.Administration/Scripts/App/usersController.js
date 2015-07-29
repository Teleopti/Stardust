(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('usersController', usersController, []);

	function usersController($http) {
		var vm = this;
		var tokenKey = 'accessToken';

		vm.token = sessionStorage.getItem(tokenKey);
		if (vm.token === null) {
			return;
		}

		function getHeaders() {
			return {
				headers: { 'Authorization': 'Bearer ' + vm.token }
			};
		}

		vm.LoadUsers = function () {
			$http.get('./Users', getHeaders())
				.success(function (data) {
					vm.users = data;
					
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.LoadUsers();
		
	}

})();