(function () {
    'use strict';

    angular
        .module('wfm.rta')
        .controller('AdjustAdherenceController', AdjustAdherenceController);

    function AdjustAdherenceController() {
        var vm = this;
        vm.showAdjustToNeutralForm = false;
        
        vm.toggleAdjustToNeutralForm = function() {
            vm.showAdjustToNeutralForm = !vm.showAdjustToNeutralForm;
        }
    }
})();
