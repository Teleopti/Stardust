(function() {
    'use strict';
    angular.module('wfm.teamSchedule').directive('commandCheck', commandCheckDirective);

    function commandCheckDirective() {
        return {
            restrict: 'E',
            scope: {},
            controller: commandCheckCtrl,
            controllerAs: 'vm',
            bindToController: true,
            templateUrl: 'js/teamSchedule/html/commandCheck.tpl.html',
            require: ['^teamscheduleCommandContainer', 'commandCheck']
        };
    }

    commandCheckCtrl.$inject = ['$scope', 'CommandCheckService', 'PersonSelection'];

    function commandCheckCtrl($scope, CommandCheckService, personSelectionSvc) {
        var vm = this;

        vm.overlappingAgents = CommandCheckService.getOverlappingAgentList();
        var unselectedPersonIds = vm.overlappingAgents.map(function(agent) {
            return agent.PersonId;
        });
        
        personSelectionSvc.unselectPersonsWithIds(unselectedPersonIds);

        vm.applyCommandFix = function() {
        	CommandCheckService.completeCommandCheck();
        };
    }
})();