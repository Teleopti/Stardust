(function () {
    'use strict';

    angular
        .module('wfm.rta')
        .controller('AdjustAdherenceController', AdjustAdherenceController);

    AdjustAdherenceController.$inject = ['CurrentUserInfo'];

    function AdjustAdherenceController(currentUserInfo) {
        var vm = this;
        vm.showAdjustToNeutralForm = false;
        
        vm.startDate = moment(new Date()).add(-1, 'days');
        vm.endDate = moment(new Date()).add(-1, 'days');
        
        vm.startTime = moment(new Date()).set({h: 8, m: 0});
        vm.endTime = moment(new Date()).set({h: 18, m: 0});
        
        currentUserInfo.Load().then(function(data){
            vm.showMeridian = data.DateTimeFormat.ShowMeridian;
        });
        
        vm.toggleAdjustToNeutralForm = function() {
            vm.showAdjustToNeutralForm = !vm.showAdjustToNeutralForm;
        }
    }
})();
