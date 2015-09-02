(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('usersController', usersController, ['tokenHeaderService']);

	function usersController($http, tokenHeaderService) {
		var vm = this;

		vm.LoadUsers = function () {
			$http.get('./Users', tokenHeaderService.getHeaders())
				.success(function (data) {
					vm.users = data;
					
				}).error(function (xhr, ajaxOptions, thrownError) {
					console.log(xhr.status + xhr.responseText + thrownError);
				});
		}

		vm.LoadUsers();
		
	}

})();