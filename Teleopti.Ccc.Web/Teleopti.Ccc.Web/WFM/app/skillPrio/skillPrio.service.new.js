(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .factory('skillPrioServiceNew', skillPrioService);

    skillPrioService.$inject = ['$resource'];
    function skillPrioService($resource) {
        var adminSkillRoutingActivity = $resource('../api/ResourcePlanner/AdminSkillRoutingActivity');
        var adminSkillRoutingPriority = $resource('../api/ResourcePlanner/AdminSkillRoutingPriority');
        var adminSkillRoutingPriorityPost = $resource('../api/ResourcePlanner/AdminSkillRoutingPriorityPost');

        var service = {
            getActivites: adminSkillRoutingActivity,
            getSkills: adminSkillRoutingPriority,
            saveSkills: adminSkillRoutingPriorityPost
        };
        return service;
    }
})();
