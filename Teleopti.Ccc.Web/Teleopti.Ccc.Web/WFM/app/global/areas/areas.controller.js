(function() {
    'use strict';

    angular
        .module('wfm.areas', [])
        .controller('AreasController', AreasController);

    AreasController.$inject = ['$scope', 'areasService'];

    function AreasController($scope, areasService) {
        var vm = this;
        vm.areas = [];
    		var filters = [];

        vm.loadAreas = function() {
    			areasService.getAreas().then(function (result) {
    				for (var i = 0; i < result.length; i++) {
    					result[i].filters = [];
    				}
    				vm.areas = result;
    				vm.areasLoaded = true;
    			});
    		};

        vm.unauthModal = true;

    		vm.dismissUnauthModal = function () {
    		    window.history.back();
    		};

    }
})();
