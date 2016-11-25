describe('permissionsFilter', function(){
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

    var filter = $filter('selectedFunctionsFilter');
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
          IsSelected: true,
          ChildFunctions: []
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

    var filter = $filter('selectedFunctionsFilter');
    var filteredArray = filter(vm.applicationFunctions);

    expect(filteredArray.length).toEqual(1);
  });

  it('should filter out all selected children', function(){
    fakeBackend
    .withApplicationFunction({
      FunctionCode: 'Raptor',
      FunctionDescription: 'xxOpenRaptorApplication',
      FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
      IsDisabled: false,
      LocalizedFunctionDescription: 'Open Teleopti WFM',
      IsSelected: true,
      ChildFunctions: [
        {
          Id: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
          Name: 'Child1',
          IsSelected: false,
          ChildFunctions: []
        },
        {
          Id: 'ca1af3de-7544-41b9-bd5d-579d85e5d5ad',
          Name: 'Child2',
          IsSelected: true,
          ChildFunctions: []
        },
        {
          Id: 'a695b002-eda8-42ef-ba19-3388ead3ea2b',
          Name: 'Child3',
          IsSelected: true,
          ChildFunctions: []
        }
      ]
    });
    $httpBackend.flush();

    var filter = $filter('unSelectedFunctionsFilter');
    var filteredArray = filter(vm.applicationFunctions);
    expect(filteredArray.length).toEqual(1);
    expect(filteredArray[0].ChildFunctions.length).toEqual(1);
  });

  it('should show selected parent with unselected children', function(){
    fakeBackend
    .withApplicationFunction({
      FunctionCode: 'GrandParent1',
      FunctionDescription: 'GrandParent1',
      FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
      IsDisabled: false,
      LocalizedFunctionDescription: 'GrandParent1',
      IsSelected: true,
      ChildFunctions: [
        {
          FunctionCode: 'Parent1',
          FunctionDescription: 'Parent1',
          FunctionId: 'cf07e1ea-204a-4c20-a0cf-48579f09e6a4',
          IsDisabled: false,
          LocalizedFunctionDescription: 'Parent1',
          IsSelected: true,
          ChildFunctions: [
            {
              FunctionCode: 'Child1',
              FunctionDescription: 'Child1',
              FunctionId: '0d8554a3-e5b9-4d1d-ad11-1b32f9878c31',
              IsDisabled: false,
              LocalizedFunctionDescription: 'Child1',
              IsSelected: true,
              ChildFunctions: [
                {
                  FunctionCode: 'GrandChild1',
                  FunctionDescription: 'GrandChild1',
                  FunctionId: '942150e9-d0eb-46b7-891e-ff94f41166ea',
                  IsDisabled: false,
                  LocalizedFunctionDescription: 'GrandChild1',
                  IsSelected: false,
                  ChildFunctions: []
                },
                {
                  FunctionCode: 'GrandChild2',
                  FunctionDescription: 'GrandChild2',
                  FunctionId: '52998389-fac4-42a9-b5d6-82ae4b0c8567',
                  IsDisabled: false,
                  LocalizedFunctionDescription: 'GrandChild2',
                  IsSelected: true,
                  ChildFunctions: []
                }
              ]
            },
            {
              FunctionCode: 'Child2',
              FunctionDescription: 'Child2',
              FunctionId: 'cd36b81e-6f9e-43da-ad75-e52bb5a65ca8',
              IsDisabled: false,
              LocalizedFunctionDescription: 'Child2',
              IsSelected: false,
              ChildFunctions: []
            }
          ]
        },
        {
          FunctionCode: 'Parent2',
          FunctionDescription: 'Parent2',
          FunctionId: 'fdf17d09-e44c-47ab-b053-3afe2cc1a884',
          IsDisabled: false,
          LocalizedFunctionDescription: 'Parent2',
          IsSelected: true,
          ChildFunctions: []
        }
      ]
    });
    $httpBackend.flush();
    var filter = $filter('unSelectedFunctionsFilter');
    var filteredArray = filter(vm.applicationFunctions);

    console.log(filteredArray);

    expect(filteredArray.length).toEqual(1);
    expect(filteredArray[0].ChildFunctions.length).toBe(1);
    expect(filteredArray[0].FunctionId).toEqual('f19bb790-b000-4deb-97db-9b5e015b2e8c');
  });


})
