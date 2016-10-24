(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioSkillService', skillPrioSkillService);

    skillPrioSkillService.$inject = ['skillPrioService'];
    function skillPrioSkillService(skillPrioService) {
        this.getSkills = getSkills;
        this.saveSkills = saveSkills;

        ////////////////
        function getSkills() {
            return skillPrioService.getAdminSkillRoutingPriority;
        }
        function saveSkills(){
            return skillPrioService.postAdminSkillRoutingPriorityPost;
        }
    }
})();