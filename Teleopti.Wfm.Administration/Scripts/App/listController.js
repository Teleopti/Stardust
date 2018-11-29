
(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('listController', listController, ['tokenHeaderService']);

	function listController($scope, $http, tokenHeaderService) {
		var vm = this;

		vm.ToggleNameToSave="";
		vm.ToggleValueToSave="false";
		
		var getAllOverrides = function(){
			$http.get("./Toggle/AllOverrides", tokenHeaderService.getHeaders()).then(function (data) {
				$scope.Overrides = data.data;
			});
		};
		
		var getAllToggleNamesWithoutOverride = function(){
			$http.get("./Toggle/AllToggleNamesWithoutOverrides", tokenHeaderService.getHeaders()).then(function (data) {
				$scope.ToggleNames = data.data;
				if(data.length>0)
					vm.ToggleNameToSave = data[0];
			});	
		};
		
		vm.DeleteOverride = function(override){
			$http.delete('./Toggle/DeleteOverride/'+override.Toggle, tokenHeaderService.getHeaders())
				.then(function(data) {
					getAllOverrides(); 
					getAllToggleNamesWithoutOverride();
					alert("Toggles changes made here may take some time before being active in system!");
				});	
		};
		
		vm.ShowAdd = false;
		vm.ToggleAdd = function(){
			vm.ShowAdd=!vm.ShowAdd;
		};
		
		vm.SaveOverride = function(){
			$http.post('./Toggle/SaveOverride', {toggle: vm.ToggleNameToSave, value: vm.ToggleValueToSave}, tokenHeaderService.getHeaders())
				.then(function(data) {
					getAllOverrides();
					getAllToggleNamesWithoutOverride();
					vm.ShowAdd = false;
					vm.ToggleValueToSave="false";
					alert("Toggles changes made here may take some time before being active in system!");
				});
		};

		$http.get("./AllTenants", tokenHeaderService.getHeaders()).then(function(data) {
			$scope.Tenants = data;
		});

		$http.get("./AllConfigurations", tokenHeaderService.getHeaders()).then(function (data) {
			$scope.Configurations = data;
		});


		getAllToggleNamesWithoutOverride();
		getAllOverrides();
	}

})();
