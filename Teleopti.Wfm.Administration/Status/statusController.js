(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("statusController", statusController);

	function statusController($http) {
		var vm = this;

		vm.statusSteps = [];
		
		$http.get('./status/listCustom')
			.then(function(response){
				vm.statusSteps = response.data.map(function(step){
					return {
						id: step.Id,
						name: step.Name,
						description: step.Description,
						pingUrl: step.PingUrl,
						limit: step.Limit
					};					
				});
			});
	}
})();