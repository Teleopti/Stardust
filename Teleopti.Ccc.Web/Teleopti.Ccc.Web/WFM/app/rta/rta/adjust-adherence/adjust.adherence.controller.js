(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAdjustAdherenceToNeutralController', RtaAdjustAdherenceToNeutralController);

	function RtaAdjustAdherenceToNeutralController() {

        var vm = this;
        vm.adjustToNeutralStartTime = new Date();
        vm.adjustToNeutralEndTime = new Date();

        vm.toggleStartSelection = function() {
            vm.displayAdjustToNeutralStart = !vm.displayAdjustToNeutralStart;
            vm.displayAdjustToNeutralEnd = false;
        }; 
        
        vm.toggleEndSelection = function() {
            vm.displayAdjustToNeutralEnd = !vm.displayAdjustToNeutralEnd;
            vm.displayAdjustToNeutralStart = false;
        }
    }
})();
