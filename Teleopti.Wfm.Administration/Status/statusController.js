(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("statusController", statusController);

	function statusController($http) {
		var vm = this;

		vm.statusSteps = [];

		$http.get('./status/list')
			.then(function(response){
				vm.statusSteps = response.data;
			})
	}
})();