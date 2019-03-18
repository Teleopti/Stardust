(function () {
	"use strict";

	angular
		.module("adminApp")
		.controller("statusController", statusController);

	function statusController($http) {
		var vm = this;

		vm.statusSteps = [];
		
		vm.newStatusStep = {
			limit: 60,
			name:''
		};
		
		vm.newStatusStepValid = function(){
			return !isNaN(vm.newStatusStep.limit) && vm.newStatusStep.name !== '';
		};

		vm.storeNew = function(){
			if(vm.newStatusStepValid()){
				$http.post('./status/Add', {
					Name: vm.newStatusStep.name,
					Description: vm.newStatusStep.description,
					LimitInSeconds: vm.newStatusStep.limit
				});
			}
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