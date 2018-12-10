(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('usersController', usersController, ['tokenHeaderService']);

	function usersController($http, tokenHeaderService) {
		var vm = this;

		vm.LoadUsers = function() {
			$http.get('./Users', tokenHeaderService.getHeaders())
				.then(function (response) {
					vm.users = response.data;
				});
		};

		vm.LoadUsers();
	}
})();