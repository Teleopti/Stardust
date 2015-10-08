(function () {
  'use strict';
  var rtaAgentsFilters = angular.module('wfm.rta');

  rtaAgentsFilters.filter('agentFilter', [
    function () {
    return function (agents, filterText){
      console.log("HELLO");
    var filteredAgents = [];
    var filterTextMatch = new RegExp(filterText, 'i');
    for (var i = 0; i < agents.length; i++){
      var agent = agents[i];
      if (filterTextMatch.test(agent.substring(0,1))) {
        filteredAgents.push(agent);
      }
    }
    return filteredAgents;
  };
  }]);
})();
