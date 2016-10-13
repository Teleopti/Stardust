(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioService', skillPrioService);

    skillPrioService.$inject = ['$resource', '$q'];
    function skillPrioService($resource, $q) {
        this.getAdminSkillRoutingActivity = getAdminSkillRoutingActivity;
        this.getAdminSkillRoutingPriority = getAdminSkillRoutingPriority;

        ////////////////

        function getAdminSkillRoutingActivity() {
            return $resource('../api/ResourcePlanner/AdminSkillRoutingActivity', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        }

        function getAdminSkillRoutingPriority() {
            return $resource('../api/ResourcePlanner/AdminSkillRoutingPriority', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        }

    }
})();