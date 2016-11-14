'use strict';
//DONT REMOVE X
xdescribe('component: permissionsList', function () {
  var $httpBackend,
  fakeBackend,
  $controller,
  $componentController,
  ctrl,
  vm;

  beforeEach(function () {
    module('wfm.permissions');
  });

  beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$componentController_, _$controller_) {
    $httpBackend = _$httpBackend_;
    fakeBackend = _fakePermissionsBackend_;
    $componentController = _$componentController_;
    $controller = _$controller_;

    fakeBackend.clear();
    vm = $controller('PermissionsCtrlRefact');
  }));

  afterEach(function () {
    $httpBackend.verifyNoOutstandingExpectation();
    $httpBackend.verifyNoOutstandingRequest();
  });

  it('should be able to select BusinessUnit', function () {
    var BusinessUnit = {
      ChildNodes: [],
      Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
      Name: "TeleoptiCCCDemo",
      Type: "BusinessUnit"
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

    expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
  });

  it('should be able to deselect BusinessUnit', function () {
    var BusinessUnit = {
      ChildNodes: [],
      Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
      Name: "TeleoptiCCCDemo",
      Type: "BusinessUnit",
      IsSelected: true
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

    expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
  });

  it('should select businessunit when selecting site', function () {
    var BusinessUnit = {
      Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
      Name: "TeleoptiCCCDemo",
      Type: "BusinessUnit",
      ChildNodes: [{
        ChildNodes: [],
        Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
        Name: "London",
        Type: "Site"
      }]
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);

    expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
  });

  it('should select businessunit and site when selecting team', function () {
    var BusinessUnit = {
      ChildNodes: [{
        Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
        Name: "TeleoptiCCCDemo",
        Type: "BusinessUnit",
        ChildNodes: [{
          Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
          Name: "Team 1",
          Type: "Team"
        }],
        Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
        Name: "London",
        Type: "Site"
      }]
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);

    expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(true);
  });

  it('should select all teams when clicking parent site', function () {
    var BusinessUnit = {
      ChildNodes: [{
        Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
        Name: "TeleoptiCCCDemo",
        Type: "BusinessUnit",
        ChildNodes: [{
          Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
          Name: "Team 1",
          Type: "Team"
        },
        {
          Id: "360bb004-356f-44fa-be18-cb92aa84c937",
          Name: "Team 2",
          Type: "Team"
        }],
        Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
        Name: "London",
        Type: "Site"
      }]
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);

    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(true);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(true);
  });

  it('should deselect all teams when clicking parent site', function () {
    var BusinessUnit = {
      ChildNodes: [{
        Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
        Name: "TeleoptiCCCDemo",
        Type: "BusinessUnit",
        ChildNodes: [{
          Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
          Name: "Team 1",
          Type: "Team",
          IsSelected: true
        },
        {
          Id: "360bb004-356f-44fa-be18-cb92aa84c937",
          Name: "Team 2",
          Type: "Team",
          IsSelected: true
        }],
        Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
        Name: "London",
        Type: "Site",
        IsSelected: true
      }]
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);

    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(false);
  });

  it('should deselect paret when no children selected', function () {
    var BusinessUnit = {
      ChildNodes: [{
        Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
        Name: "TeleoptiCCCDemo",
        Type: "BusinessUnit",
        ChildNodes: [{
          Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
          Name: "Team 1",
          Type: "Team",
          IsSelected: true
        },
        {
          Id: "360bb004-356f-44fa-be18-cb92aa84c937",
          Name: "Team 2",
          Type: "Team",
          IsSelected: true
        }],
        Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
        Name: "London",
        Type: "Site",
        IsSelected: true
      }]
    };
    var DynamicOptions = [];
    fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
    $httpBackend.flush();
    ctrl = $componentController('permissionsList', null, { org: vm.organizationSelection});

    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
    ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1]);

    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
    expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(false);
  });

});
