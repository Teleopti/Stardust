(function() {
  'use strict';

  angular
  .module('wfm.areas', ['ngResource'])
  .controller('AreasController', AreasController);

  AreasController.$inject = ["$q","areasService", "Toggle"];

  function AreasController($q, areasService, toggleService) {
    var vm = this;
    vm.areas = [];
    var filters = [];

    vm.loadAreas = function() {
        areasService.getAreas().then(function (result) {
            console.log(result);
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

    vm.toggleMobileMenu = function () {
      if (vm.detectMobile() == false && vm.mainMenuState) {
        vm.mainMenuState = false;
      }
	}
	  $q.all([toggleService.togglesLoaded]).then(function() {
		  vm.myTimeLinkEnabled = toggleService.Wfm_AddMyTimeLink_45088;
	  });

    vm.loadAreas();
    vm.unauthModal = true;
    vm.mainMenuState = vm.detectMobile();

    vm.dismissUnauthModal = function () {
      window.history.back();
    };

  }
})();
