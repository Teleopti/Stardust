'use strict';
describe('RtaToolController', function () {
  var
    $httpBackend,
    $RtaToolControllerBuilder,
    scope,
    fakeBackend,
    vm;

  beforeEach(module('wfm.rtaTool'));

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
      DataSource: '1'
    });

    vm = $RtaToolControllerBuilder.createController().vm;

    expect(vm.agents.length).toEqual(1);
  });

  it('should get phone states', function () {
    fakeBackend.withPhoneState({
      Code: 'Ready',
      Name: 'Ready'
    });

    vm = $RtaToolControllerBuilder.createController().vm;

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

    vm = $RtaToolControllerBuilder.createController().vm;

    expect(vm.agents[0].Name).toEqual('John Smith');
    expect(vm.agents[0].UserCode).toEqual('0019');
    expect(vm.agents[0].DataSource).toEqual('1');
    expect(vm.agents[0].StateCodes.length).toEqual(1);
    expect(vm.agents[0].StateCodes[0].Code).toEqual('Ready');
    expect(vm.agents[0].StateCodes[0].Name).toEqual('Ready');
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
    vm = $RtaToolControllerBuilder.createController().vm;

    vm.agents[0].sendState(vm.agents[0].StateCodes[0]);
    $httpBackend.flush();

    expect(requestBody.AuthenticationKey).toEqual('!#¤atAbgT%');
    expect(requestBody.IsSnapshot).toEqual(false);
    expect(requestBody.SourceId).toEqual('1');
    expect(requestBody.States.length).toEqual(1);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode).toEqual('Ready');
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
    vm = $RtaToolControllerBuilder.createController().vm;

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
    vm = $RtaToolControllerBuilder.createController().vm;
    vm.agents[0].selectAgent();
    vm.agents[1].selectAgent();

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
    vm = $RtaToolControllerBuilder.createController().vm;
    vm.agents[0].selectAgent();
    vm.agents[1].selectAgent();
    vm.agents[0].selectAgent();

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
    vm = $RtaToolControllerBuilder.createController().vm;

    vm.toggleAgents();

    expect(vm.agents[0].isSelected).toEqual(true);
    expect(vm.agents[1].isSelected).toEqual(true);
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
    vm = $RtaToolControllerBuilder.createController().vm;

    vm.toggleAgents();
    vm.toggleAgents();

    expect(vm.agents[0].isSelected).toEqual(false);
    expect(vm.agents[1].isSelected).toEqual(false);
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
    vm = $RtaToolControllerBuilder.createController().vm;

    vm.agents[0].selectAgent();
    vm.toggleAgents();

    expect(vm.agents[0].isSelected).toEqual(true);
    expect(vm.agents[1].isSelected).toEqual(true);
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
    var scope = $RtaToolControllerBuilder.createController();
    var vm = scope.vm;

    scope
    .apply(vm.togglePause())
    .wait(5000)

    expect(requestBody.States.length).toEqual(1);
    expect(requestBody.States[0].UserCode).toEqual('0019');
    expect(requestBody.States[0].StateCode).toEqual('Ready');
  });

});
