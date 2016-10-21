(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioAggregator', skillPrioAggregator);

    skillPrioAggregator.$inject = ['$q', 'skillPrioActivityService', 'skillPrioSkillService'];
    function skillPrioAggregator($q, skillPrioActivityService, skillPrioSkillService) {
        this.getSkillsForActivity = getSkillsForActivity;
        this.getSkills = getSkills;
        this.getActivitys = getActivitys;
        ////////////////

        function getActivitys() {
            return skillPrioActivityService.getActivitys();
        }
        function getSkills() {
            return skillPrioSkillService.getSkills();
        }

        function matchSkillsForActivity(activity) {
            if(!activity) return;
            var deferred = $q.defer();
            var aggregatedSkills = [];
            var skills = getSkills().query();
            skills.$promise.then(function () {
                skills.forEach(function (skill) {
                    if (skill.ActivityGuid === activity.ActivityGuid) {
                        aggregatedSkills.push(skill);
                    }
                });
                deferred.resolve(aggregatedSkills);
            });
            return aggregatedSkills;

        }

        function getSkillsForActivity(activity) {
            return matchSkillsForActivity(activity);
        }
    }
})();