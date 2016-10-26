(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioActivityService', skillPrioActivityService);

    skillPrioActivityService.$inject = ['skillPrioService'];
    function skillPrioActivityService(skillPrioService) {
        this.getActivitys = getActivitys;
        ////////////////
        
        function getActivitys() {
            return skillPrioService.getAdminSkillRoutingPriority;
        }

    }
})();