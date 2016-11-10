xdescribe('permissionsFilter', function(){
  var $httpBackend,
    fakeBackend,
    $controller,
    $filter,
    vm;

  beforeEach(function () {
    module('wfm.permissions');
  });

  beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$controller_,  _$filter_) {
    $httpBackend = _$httpBackend_;
    fakeBackend = _fakePermissionsBackend_;
    $controller = _$controller_;
    $filter = _$filter_;
    fakeBackend.clear();
    vm = $controller('PermissionsCtrlRefact');
  }));

  afterEach(function () {
    $httpBackend.verifyNoOutstandingExpectation();
    $httpBackend.verifyNoOutstandingRequest();
  });

  it('should show only selected functions', function(){
    fakeBackend
    .withApplicationFunction({
      ChildFunctions: [],
      FunctionCode: 'Raptor',
      FunctionDescription: 'xxOpenRaptorApplication',
      FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
      IsDisabled: false,
      LocalizedFunctionDescription: 'All',
      IsSelected: false
    }).withApplicationFunction({
      ChildFunctions: [],
      FunctionCode: 'Raptor',
      FunctionDescription: 'xxOpenRaptorApplication',
      FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
      IsDisabled: false,
      LocalizedFunctionDescription: 'Open Teleopti WFM',
      IsSelected: true
    });
    $httpBackend.flush();

    var filter = $filter('showOnlySelectedFunctionsFilter');
    var filteredArray = filter(vm.applicationFunctions);

    expect(filteredArray.length).toEqual(1);
  });

  it('should show only selected functions with children', function(){
    fakeBackend
    .withApplicationFunction({
      ChildFunctions: [],
      FunctionCode: 'Raptor',
      FunctionDescription: 'xxOpenRaptorApplication',
      FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
      IsDisabled: false,
      LocalizedFunctionDescription: 'All',
      IsSelected: false
    }).withApplicationFunction({
      ChildFunctions: [
        {
          FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
          IsSelected: true
        }
      ],
      FunctionCode: 'Raptor',
      FunctionDescription: 'xxOpenRaptorApplication',
      FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
      IsDisabled: false,
      LocalizedFunctionDescription: 'Open Teleopti WFM',
      IsSelected: true
    });
    $httpBackend.flush();

    var filter = $filter('showOnlySelectedFunctionsFilter');
    var filteredArray = filter(vm.applicationFunctions);

    expect(filteredArray.length).toEqual(2);
  });


})
