(function() {
	'use strict';

	angular.module('wfm.areas', ['ngResource']).controller('AreasController', AreasController);

	AreasController.$inject = ['$q', 'areasService', '$scope'];

	function AreasController($q, areasService, $scope) {
		var vm = this;
		vm.areas = [];

		vm.$onInit = function() {
			console.log('oninit');
			const mq = window.matchMedia('(max-width: 768px)');
			mq.addListener(function(mq) {
				if (mq.matches) {
					vm.mainMenuState = false;
					$scope.$apply();
				}
			});
		};

		vm.loadAreas = function() {
			areasService.getAreasWithPermission().then(function(result) {
				for (var i = 0; i < result.length; i++) {
					result[i].filters = [];
				}
				vm.areas = result;
				vm.areasLoaded = true;
			});
		};

		vm.detectMobile = function() {
			return window.innerWidth > 768 ? true : false;
		};

		vm.toggleMobileMenu = function() {
			if (vm.detectMobile() == false && vm.mainMenuState) {
				vm.mainMenuState = false;
			}
		};

		vm.loadAreas();
		vm.unauthModal = true;
		vm.mainMenuState = vm.detectMobile();

		vm.dismissUnauthModal = function() {
			window.history.back();
		};
	}
})();
