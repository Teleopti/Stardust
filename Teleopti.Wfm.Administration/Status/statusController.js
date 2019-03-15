(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("statusController", statusController);

	function statusController($http) {
		var vm = this;

		vm.statusSteps = [];
		
		vm.newStatusStep = {};
		
		vm.storeNew = function(){
			$http.post('./status/Add', vm.newStatusStep);
		};
		
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