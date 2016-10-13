(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioService', skillPrioService);

    skillPrioService.$inject = ['$resource','$q'];
    function skillPrioService($resource, $q) {
        this.getAdminSkillRoutingActivity = getAdminSkillRoutingActivity;
        this.getSkills = getSkills;

        ////////////////

        function getAdminSkillRoutingActivity() {
            return $resource('../api/ResourcePlanner/AdminSkillRoutingActivity', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        }

        function getSkills() {
            return ['test skill'];
        }

    }
})();