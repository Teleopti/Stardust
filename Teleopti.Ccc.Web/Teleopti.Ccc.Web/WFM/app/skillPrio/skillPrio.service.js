// (function () {
//     'use strict';

//     angular
//         .module('wfm.skillPrio')
//         .service('skillPrioService', skillPrioService);

//     skillPrioService.$inject = ['$resource', '$q'];
//     function skillPrioService($resource, $q) {
//         this.getAdminSkillRoutingActivity = getAdminSkillRoutingActivity;

//         ///////////////////
//         ////should be factory!/////
//         //////////////////

//         function getAdminSkillRoutingActivity() {
//             return $resource('../api/ResourcePlanner/AdminSkillRoutingActivity', {}, {
//                 query: {
//                     method: 'GET',
//                     isArray: true
//                 }
//             }).query().$promise;
//         }


//     }
// })();

(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .factory('skillPrioService', skillPrioService);

    skillPrioService.$inject = ['$resource'];
    function skillPrioService($resource) {
        var adminSkillRoutingActivity = $resource('../api/ResourcePlanner/AdminSkillRoutingActivity');
        var adminSkillRoutingPriority = $resource('../api/ResourcePlanner/AdminSkillRoutingPriority');
        var adminSkillRoutingPriorityPost = $resource('../api/ResourcePlanner/AdminSkillRoutingPriorityPost');
        ////////////////

        var service = {
            getAdminSkillRoutingActivity: adminSkillRoutingActivity, //getActivites
            getAdminSkillRoutingPriority: adminSkillRoutingPriority, //getSkills
            postAdminSkillRoutingPriorityPost: adminSkillRoutingPriorityPost //saveSkills
        };

        return service;



    }
})();