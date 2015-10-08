'use strict';
describe('RtaAgentsFilters', function () {

  beforeEach(module('wfm'));


  xit('should filter on the input string', inject(function ($filter) {
    var agents = [{Name: 'Julian Feldman', TeamName: 'Students'},
    {Name: 'Kevin Glancy', TeamName: 'Students'}];

    var filter = $filter('agentFilter');

    var filteredAgents = filter(agents, 'Julian');

    expect(filteredAgents.length).toEqual(1);
  }));
});
