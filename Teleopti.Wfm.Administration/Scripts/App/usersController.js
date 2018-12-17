(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('usersController', usersController);

	function usersController($http) {
		var vm = this;

		vm.LoadUsers = function() {
			$http.get('./Users')
				.then(function (response) {
					vm.users = response.data;
				});
		};

		vm.LoadUsers();
	}
})();