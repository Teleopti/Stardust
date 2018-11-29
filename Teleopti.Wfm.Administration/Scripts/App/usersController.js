(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('usersController', usersController, ['tokenHeaderService']);

	function usersController($http, tokenHeaderService) {
		var vm = this;

		vm.LoadUsers = function() {
			$http.get('./Users', tokenHeaderService.getHeaders())
				.then(function(data) {
					vm.users = data;

				}).catch(function(xhr, ajaxOptions, thrownError) {
					console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
				});
		};

		vm.LoadUsers();
		
	}

})();