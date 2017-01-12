(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('staffingController', staffingController);

    staffingController.inject = ['staffingService'];
    function staffingController(staffingService) {
        var vm = this;
        vm.staffingArea =
            vm.staffing = monitorskillstaffing.get();
        vm.getStaffingForSkillArea = getStaffingForSkillArea(areaId);


        ////////////////

        getSkillAreaStaffing = function (areaId) {
            return staffingService.getSkillAreaStaffing.get(areaId);
        }
        getSkillStaffing = function(skillId){
            return staffingService.getSkillAreaStaffing.get(skillId)
        }
    }
})();