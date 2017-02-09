(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .service('resourcePlannerRouteService', Service);

    Service.$inject = ['$state'];

    /* @ngInject */
    function Service($state) {
        var service = {
          goToAgentGroup: goToAgentGroup
        }

        return service;

        function goToAgentGroup(groupId) {
          if (groupId) {
            $state.go('resourceplanner.oneagentroup', { groupId: groupId });
          }
        }
    }
})();
