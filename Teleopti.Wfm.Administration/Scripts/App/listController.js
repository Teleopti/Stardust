
(function () {
	'use strict';

	angular
		.module('adminApp')
		.controller('listController', listController);

	function listController($scope, $http) {
		var vm = this;

		vm.ToggleNameToSave = "";
		vm.ToggleValueToSave = "false";

		var getAllOverrides = function () {
			$http.get("./Toggle/AllOverrides")
				.then(function (response) {
					$scope.Overrides = response.data;
				});
		};

		var getAllToggleNamesWithoutOverride = function () {
			$http.get("./Toggle/AllToggleNamesWithoutOverrides")
				.then(function (response) {
					$scope.ToggleNames = response.data;
					if (response.data.length > 0)
						vm.ToggleNameToSave = response.data[0];
				});
		};

		vm.DeleteOverride = function (override) {
			$http.delete('./Toggle/DeleteOverride/' + override.Toggle)
				.then(function (response) {
					getAllOverrides();
					getAllToggleNamesWithoutOverride();
					alert("Toggles changes made here may take some time before being active in system!");
				});
		};

		vm.ShowAdd = false;
		vm.ToggleAdd = function () {
			vm.ShowAdd = !vm.ShowAdd;
		};

		vm.SaveOverride = function () {
			$http.post('./Toggle/SaveOverride', { toggle: vm.ToggleNameToSave, value: vm.ToggleValueToSave })
				.then(function (response) {
					getAllOverrides();
					getAllToggleNamesWithoutOverride();
					vm.ShowAdd = false;
					vm.ToggleValueToSave = "false";
					alert("Toggles changes made here may take some time before being active in system!");
				});
		};

		$http.get("./AllTenants")
			.then(function (response) {
				$scope.Tenants = response.data;
			});

		$http.get("./AllConfigurations")
			.then(function (response) {
				$scope.Configurations = response.data;
			});


		getAllToggleNamesWithoutOverride();
		getAllOverrides();
	}

})();
