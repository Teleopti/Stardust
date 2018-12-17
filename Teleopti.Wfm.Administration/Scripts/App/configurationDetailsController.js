(function () {
	'use strict';
	angular
		.module('adminApp')
		.controller('configurationDetailsController', configurationDetailsController);

	function configurationDetailsController($http, $routeParams) {
		var vm = this;

		vm.Key = $routeParams.key;
		vm.Value = "";
		vm.SaveEnabled = true;
		vm.example = "";

		vm.Message = "";

		var loadConfiguration = function() {
			$http.post('./Configuration', '"' + vm.Key + '"')
				.then(function(response) {
					vm.Value = response.data;
				});
		};

		loadConfiguration();

		var check = function() {
			if (vm.Key === "FrameAncestors") {
				return vm.example = "ex: http://anothersite.teleopti.com";
			} else {
				return vm.example = vm.Key;
			}
		};

		check();

		vm.save = function () {
			$http.post('./SaveConfiguration', {
				Key: vm.Key,
				Value: vm.Value
			})
				.then(function (response) {
					if (!response.data.Success) {
						vm.Message = response.data.Message;
						return;
					}

					window.location = "#/";
				})
				.catch(function (xhr, ajaxOptions, thrownError) {
					vm.Message = xhr.Message + ': ' + xhr.ExceptionMessage;
					vm.Success = false;
				});
		};
	}
})();