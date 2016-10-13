(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioListService', skillPrioListService);

    skillPrioListService.$inject = ['skillPrioService'];
    function skillPrioListService(skillPrioService) {
        this.getActivitys = getActivitys;
        this.getSkills = getSkills;

        ////////////////
        function getActivitys() {

            return skillPrioService.getAdminSkillRoutingActivity().then(function (response) {
                return response;
            });
        }

        function getSkills() {
            return skillPrioService.getSkills();
        }

    }
})();