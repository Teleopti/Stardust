
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
			$http.get("./Toggle/AllOverrides", tokenHeaderService.getHeaders()).success(function (data) {
				$scope.Overrides = data;
			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});
		};
		
		var getAllToggleNamesWithoutOverride = function(){
			$http.get("./Toggle/AllToggleNamesWithoutOverrides", tokenHeaderService.getHeaders()).success(function (data) {
				$scope.ToggleNames = data;
				if(data.length>0)
					vm.ToggleNameToSave = data[0];
			}).error(function (xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});	
		};
		
		vm.DeleteOverride = function(override){
			$http.delete('./Toggle/DeleteOverride/'+override.Toggle, tokenHeaderService.getHeaders())
				.success(function(data) {
					getAllOverrides(); 
					getAllToggleNamesWithoutOverride();
				}).error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});	
		};
		
		vm.ShowAdd = false;
		vm.ToggleAdd = function(){
			vm.ShowAdd=!vm.ShowAdd;
		};
		
		vm.SaveOverride = function(){
			$http.post('./Toggle/SaveOverride', {toggle: vm.ToggleNameToSave, value: vm.ToggleValueToSave}, tokenHeaderService.getHeaders())
				.success(function(data) {
					getAllOverrides();
					getAllToggleNamesWithoutOverride();
					vm.ShowAdd = false;
					vm.ToggleValueToSave="false";
				}).error(function(xhr, ajaxOptions, thrownError) {
				console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
			});
		};

		$http.get("./AllTenants", tokenHeaderService.getHeaders()).success(function(data) {
			$scope.Tenants = data;
		}).error(function(xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

		$http.get("./AllConfigurations", tokenHeaderService.getHeaders()).success(function (data) {
			$scope.Configurations = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});


		getAllToggleNamesWithoutOverride();
		getAllOverrides();
	}

})();
