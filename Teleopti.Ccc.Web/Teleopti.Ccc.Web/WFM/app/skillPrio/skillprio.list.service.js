(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioListService', skillPrioListService);

    skillPrioListService.$inject = ['skillPrioService'];
    function skillPrioListService(skillPrioService) {
        this.getActivitys = getActivitys;
        this.getSkillsForActivity = getSkillsForActivity;

        ////////////////
        function getActivitys() {

            return skillPrioService.getAdminSkillRoutingActivity().then(function (response) {
                return response;
            });
        }

        function fetchFromServer() {
            return skillPrioService.getAdminSkillRoutingPriority().then(function (response) {
                return response;
            });
        }

        function getSkills(activity) {
            return fetchFromServer().then(function(res){
                var aggregatedSkills = [];
                res.forEach(function(skill){
                    if(skill.ActivityGuid === activity.ActivityGuid){
                        aggregatedSkills.push(skill);
                    }
                });
                return aggregatedSkills;
            });
        }

        function getSkillsForActivity(activity) {
            return getSkills(activity).then(function(res){
                return res;
            });
        }

    }
})();