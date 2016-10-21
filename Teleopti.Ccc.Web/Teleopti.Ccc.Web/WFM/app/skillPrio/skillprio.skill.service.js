(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioSkillService', skillPrioSkillService);

    skillPrioSkillService.$inject = ['skillPrioService'];
    function skillPrioSkillService(skillPrioService) {
        this.getSkills = getSkills;

        ////////////////
        function getSkills() {
            return skillPrioService.getAdminSkillRoutingPriority;
        }
    }
})();