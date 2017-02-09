(function() {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('agentGroupOneController', Controller);

    Controller.$inject = ['$stateParams','agentGroupService'];

    /* @ngInject */
    function Controller( $stateParams, agentGroupService) {
        var vm = this;
        var id = $stateParams.groupId;

        getAgentGroupbyId(id);

        function getAgentGroupbyId(id){
          var getAgentGroup = agentGroupService.getAgentGroupbyId.get({id:id});
    			return getAgentGroup.$promise.then(function(data) {
    				vm.agentGroup = data;
    				return vm.agentGroup;
    			});
        }

    }
})();
