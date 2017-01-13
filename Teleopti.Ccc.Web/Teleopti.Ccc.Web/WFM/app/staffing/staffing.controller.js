(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('staffingController', staffingController);

    staffingController.inject = ['staffingService'];
    function staffingController(staffingService) {
        var vm = this;
        vm.staffingArea = 0;
        var getSkills = getSkillStaffing;
        var getSkillsForArea = getSkillAreaStaffing

        var query = getSkills("F08D75B3-FDB4-484A-AE4C-9F0800E2F753");
        query.$promise.then(function(result){
            console.log(result)
        })
        ////////////////

        function getSkillAreaStaffing(areaId) {
            return staffingService.getSkillAreaStaffing.get(areaId);
        }
        function getSkillStaffing(skillId) {
            return staffingService.getSkillAreaStaffing.get(skillId)
        }

        var generateChart = function () {
            c3.generate({
                bindto: '#staffingChart',
                data: {
                    columns: [
                        ['data1', ],
                    ],
                    selection: {
                        enabled: true,
                    },
                },
                zoom: {
                    enabled: true,
                },
            });

        }();
    }
})();