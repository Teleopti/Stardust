(function() {
	'use strict';

	angular
		.module('wfm.areas', ['ngResource'])
		.controller('AreasController', AreasController);

	AreasController.$inject = ["$q", "areasService"];

	function AreasController($q, areasService) {
		var vm = this;
		vm.areas = [];
		vm.loadAreas = function() {
			areasService.getAreas().then(function(result) {
				for (var i = 0; i < result.length; i++) {
					result[i].filters = [];
				}
				vm.areas = result;
				vm.areasLoaded = true;
			});
		};

		vm.detectMobile = function() {
			return window.innerWidth >= 770 ? true : false;
		}

		vm.toggleMobileMenu = function() {
			if (vm.detectMobile() == false && vm.mainMenuState) {
				vm.mainMenuState = false;
			}
		}

		vm.loadAreas();
		vm.unauthModal = true;
		vm.mainMenuState = vm.detectMobile();

		vm.dismissUnauthModal = function() {
			window.history.back();
		};

	}
})();
