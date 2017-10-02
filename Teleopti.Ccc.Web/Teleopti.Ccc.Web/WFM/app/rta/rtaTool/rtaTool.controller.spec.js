'use strict';
describe('RtaToolController', function () {
  var
    $httpBackend,
    $RtaToolControllerBuilder,
    scope,
    fakeBackend,
    vm;

  var organization = {
    Sites: [
      {
        SiteName: 'London',
        SiteId: 'londonId'
      },
      {
        SiteName: 'Paris',
        SiteId: 'parisId'
      }
    ],
    Teams: [
      {
        TeamName: 'Students',
        TeamId: 'studentsId'
      },
      {
        TeamName: 'Team nigths',
        SiteId: 'teamNightsId'
      }
    ]
  }

  beforeEach(module('wfm.rtaTool'));

  beforeEach(function () {
    module(function ($provide) {
      $provide.value('organization', function () {
        return organization;
      });
    });
  });

  beforeEach(inject(function (_$httpBackend_, _RtaToolControllerBuilder_, _FakeRtaToolBackend_) {
    $httpBackend = _$httpBackend_;
    $RtaToolControllerBuilder = _RtaToolControllerBuilder_;
    fakeBackend = _FakeRtaToolBackend_;

    scope = $RtaToolControllerBuilder.setup('RtaToolController');

    fakeBackend.clear();
  }));

  it('should get agents', function () {
    fakeBackend.withAgent({
      Name: 'John Smith',
      UserCode: '0019',
      DataSource: '1',
      TeamName: 'Students',
      SiteName: 'London'
    });

    vm = $RtaToolControllerBuilder.createController(organization).vm;

    expect(vm.filteredAgents.length).toEqual(1);
  });

  it('should get agents by site', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1',
        TeamName: 'Students',
        SiteName: 'London',
        SiteId: 'londonId'
      })
      .withAgent({
        Name: 'Marcio Dias',
        UserCode: '0068',
        DataSource: '1',
        TeamName: 'Team nights',
        SiteName: 'Paris',
        SiteId: 'parisId'
      });

    var c = $RtaToolControllerBuilder.createController(organization);
    vm = c.vm;

    c.apply(function () {
      vm.organization.Sites[0].isChecked = true;
      vm.selectItems(1);
    }
    );

    expect(vm.filteredAgents.length).toEqual(1);
    expect(vm.filteredAgents[0].Name).toEqual('John Smith');
    expect(vm.filteredAgents[0].UserCode).toEqual('0019');
    expect(vm.filteredAgents[0].DataSource).toEqual('1');
    expect(vm.filteredAgents[0].TeamName).toEqual('Students');
    expect(vm.filteredAgents[0].SiteName).toEqual('London');
  });

  it('should get agents by team', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1',
        TeamName: 'Students',
        TeamId: 'studentsId'
      })
      .withAgent({
        Name: 'Marcio Dias',
        UserCode: '0068',
        DataSource: '1',
        TeamName: 'Team nights',
        SiteId: 'teamNightsId'
      });

    var c = $RtaToolControllerBuilder.createController(organization);
    vm = c.vm;

    c.apply(function () {
      vm.organization.Teams[0].isChecked = true;
      vm.selectItems(2);
    }
    );

    expect(vm.filteredAgents.length).toEqual(1);
    expect(vm.filteredAgents[0].Name).toEqual('John Smith');
    expect(vm.filteredAgents[0].UserCode).toEqual('0019');
    expect(vm.filteredAgents[0].DataSource).toEqual('1');
    expect(vm.filteredAgents[0].TeamName).toEqual('Students');
  });

  it('should reset on clear', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1',
        TeamName: 'Students',
        SiteName: 'London',
        SiteId: 'londonId'
      })
      .withAgent({
        Name: 'Marcio Dias',
        UserCode: '0068',
        DataSource: '1',
        TeamName: 'Team nights',
        SiteName: 'Paris',
        SiteId: 'parisId'
      });

    var c = $RtaToolControllerBuilder.createController(organization);
    vm = c.vm;

    c.apply(function () {
      vm.searchSite = "Lon";
      vm.organization.Sites[0].isChecked = true;
      vm.selectItems(1);
      vm.clearSelection(1);
    }
    );

    expect(vm.filteredAgents.length).toEqual(2);
    expect(vm.searchSite).toEqual('');
  });


  it('should reset on close', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1',
        TeamName: 'Students',
        SiteName: 'London',
        SiteId: 'londonId'
      })
      .withAgent({
        Name: 'Marcio Dias',
        UserCode: '0068',
        DataSource: '1',
        TeamName: 'Team nights',
        SiteName: 'Paris',
        SiteId: 'parisId'
      });

    var c = $RtaToolControllerBuilder.createController(organization);
    vm = c.vm;

    c.apply(function () {
      vm.searchSite = "Lon";
      vm.closeDropdown(1);
    }
    );

    expect(vm.searchSite).toEqual('');
  });

  it('should reset search term and close sites when opening teams', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1',
        TeamName: 'Students',
        TeamId: 'studentsId',
        SiteName: 'London',
        SiteId: 'londonId'
      })
      .withAgent({
        Name: 'Marcio Dias',
        UserCode: '0068',
        DataSource: '1',
        TeamName: 'Team nights',
        TeamId: 'teamNightsId',
        SiteName: 'Paris',
        SiteId: 'parisId'
      });

    var c = $RtaToolControllerBuilder.createController(organization);
    vm = c.vm;

    c.apply(function () {
      vm.searchSite = "Lon";
      vm.toggleDropdown(2);
    }
    );

    expect(vm.searchSite).toEqual('');
    expect(vm.openSitePicker).toEqual(false);
  });

  it('should get phone states', function () {
    fakeBackend.withPhoneState({
      Code: 'Ready',
      Name: 'Ready'
    });

    vm = $RtaToolControllerBuilder.createController(organization).vm;

    expect(vm.stateCodes.length).toEqual(1);
  });

  it('should map agents and phone states', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });

    vm = $RtaToolControllerBuilder.createController(organization).vm;

    expect(vm.filteredAgents[0].Name).toEqual('John Smith');
    expect(vm.filteredAgents[0].UserCode).toEqual('0019');
    expect(vm.filteredAgents[0].DataSource).toEqual('1');
    expect(vm.filteredAgents[0].StateCodes.length).toEqual(1);
    expect(vm.filteredAgents[0].StateCodes[0].Code).toEqual('Ready');
    expect(vm.filteredAgents[0].StateCodes[0].Name).toEqual('Ready');
  });

  it('should send single state', function () {
    var requestBody = {};
    $httpBackend.whenPOST('../Rta/State/Batch').respond(function (method, url, data, headers) {
      requestBody = JSON.parse(data);
      return [200, 1];
    });
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.filteredAgents[0].sendState(vm.filteredAgents[0].StateCodes[0]);
    $httpBackend.flush();

    expect(requestBody.AuthenticationKey).toEqual('!#¤atAbgT%');
    expect(requestBody.IsSnapshot).toEqual(false);
    expect(requestBody.SourceId).toEqual('1');
    expect(requestBody.States.length).toEqual(1);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode.Code).toEqual('Ready');
    expect(requestBody.States[0].StateCode.Name).toEqual('Ready');
  });

  it('should send batch for all agents when no agent is selcted', function () {
    var requestBody = {};
    $httpBackend.whenPOST('../Rta/State/Batch').respond(function (method, url, data, headers) {
      requestBody = JSON.parse(data);
      return [200, 1];
    });
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.stateCodes[0].sendBatch();
    $httpBackend.flush();

    expect(requestBody.AuthenticationKey).toEqual('!#¤atAbgT%');
    expect(requestBody.IsSnapshot).toEqual(false);
    expect(requestBody.SourceId).toEqual('1');
    expect(requestBody.States.length).toEqual(2);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode).toEqual('Ready');
    expect(requestBody.States[1].UserCode).toEqual('2002');
    expect(requestBody.States[1].StateCode).toEqual('Ready');
  });

  it('should send batch with selected agents', function () {
    var requestBody = {};
    $httpBackend.whenPOST('../Rta/State/Batch').respond(function (method, url, data, headers) {
      requestBody = JSON.parse(data);
      return [200, 1];
    });
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Pierre Baldi',
        UserCode: '3456',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;
    vm.filteredAgents[0].selectAgent();
    vm.filteredAgents[1].selectAgent();

    vm.stateCodes[0].sendBatch();
    $httpBackend.flush();

    expect(requestBody.States.length).toEqual(2);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode).toEqual('Ready');
    expect(requestBody.States[1].UserCode).toEqual('2002');
    expect(requestBody.States[1].StateCode).toEqual('Ready');
  });

  it('should only send batch with selected agents', function () {
    var requestBody = {};
    $httpBackend.whenPOST('../Rta/State/Batch').respond(function (method, url, data, headers) {
      requestBody = JSON.parse(data);
      return [200, 1];
    });
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;
    vm.filteredAgents[0].selectAgent();
    vm.filteredAgents[1].selectAgent();
    vm.filteredAgents[0].selectAgent();

    vm.stateCodes[0].sendBatch();
    $httpBackend.flush();

    expect(requestBody.States.length).toEqual(1);
    expect(requestBody.States[0].UserCode).toEqual('2002');
    expect(requestBody.States[0].StateCode).toEqual('Ready');
  });

  it('should select all agents', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.toggleAgents();

    expect(vm.filteredAgents[0].isSelected).toEqual(true);
    expect(vm.filteredAgents[1].isSelected).toEqual(true);
  });

  it('should select all filtered agents', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.filterText = 'joh';
    vm.filterAgents();
    vm.toggleAgents();

    expect(vm.filteredAgents.length).toEqual(1);
    expect(vm.filteredAgents[0].isSelected).toEqual(true);
  });

  it('should see all agents after removing filter', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.filterText = 'joh';
    vm.filterAgents();
    vm.filterText = '';
    vm.filterAgents();

    expect(vm.filteredAgents.length).toEqual(2);
  });

  it('should see all agents after removing filter', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.filterText = 'joh';
    vm.filterAgents();
    vm.toggleAgents();
    vm.filterText = '';
    vm.filterAgents();

    expect(vm.filteredAgents[0].isSelected).toEqual(true);
    expect(vm.filteredAgents[1].isSelected).toEqual(false);
  });

  it('should deselect all agents', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.toggleAgents();
    vm.toggleAgents();

    expect(vm.filteredAgents[0].isSelected).toEqual(false);
    expect(vm.filteredAgents[1].isSelected).toEqual(false);
  });

  it('should select all agents even if one is already selected', function () {
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withAgent({
        Name: 'Ashley Andeen',
        UserCode: '2002',
        DataSource: '1'
      });
    vm = $RtaToolControllerBuilder.createController(organization).vm;

    vm.filteredAgents[0].selectAgent();
    vm.toggleAgents();

    expect(vm.filteredAgents[0].isSelected).toEqual(true);
    expect(vm.filteredAgents[1].isSelected).toEqual(true);
  });

  it('should only send batch with selected agents', function () {
    var requestBody = {};
    $httpBackend.whenPOST('../Rta/State/Batch').respond(function (method, url, data, headers) {
      requestBody = JSON.parse(data);
      return [200, 1];
    });
    fakeBackend
      .withAgent({
        Name: 'John Smith',
        UserCode: '0019',
        DataSource: '1'
      })
      .withPhoneState({
        Code: 'Ready',
        Name: 'Ready'
      });
    var scope = $RtaToolControllerBuilder.createController(organization);
    var vm = scope.vm;

    scope
      .apply(vm.togglePause())
      .wait(5000)

    expect(requestBody.States.length).toEqual(1);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode.Code).toEqual('Ready');
    expect(requestBody.States[0].StateCode.Name).toEqual('Ready');
  });

});
