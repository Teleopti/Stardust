describe('permissionsFilter', function() {
	var $httpBackend,
		fakeBackend,
		$controller,
		$filter,
		vm;

	beforeEach(function() {
		module('wfm.permissions');
	});

	beforeEach(inject(function(_$httpBackend_, _fakePermissionsBackend_, _$controller_, _$filter_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$controller = _$controller_;
		$filter = _$filter_;

		fakeBackend.clear();
		vm = $controller('PermissionsController');
		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should show only selected functions', function() {
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
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;

		var filter = $filter('functionsFilter');
		var filteredArray = filter.selected(vm.applicationFunctions, vm.selectedFunctions);

		expect(filteredArray.length).toEqual(1);
	});

	it('should show only selected functions with children', function() {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All',
			}).withApplicationFunction({
				ChildFunctions: [{
					FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
					ChildFunctions: []
				}],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
			});
		$httpBackend.flush();
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;

		var filter = $filter('functionsFilter');
		var filteredArray = filter.selected(vm.applicationFunctions, vm.selectedFunctions);

		expect(filteredArray.length).toEqual(1);
	});

	it('should filter out all selected children', function() {
		fakeBackend
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				ChildFunctions: [{
					FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
					Name: 'Child1',
					ChildFunctions: []
				}, {
					FunctionId: 'ca1af3de-7544-41b9-bd5d-579d85e5d5ad',
					Name: 'Child2',
					ChildFunctions: []
				}, {
					FunctionId: 'a695b002-eda8-42ef-ba19-3388ead3ea2b',
					Name: 'Child3',
					ChildFunctions: []
				}]
			});
		$httpBackend.flush();
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		vm.selectedFunctions['ca1af3de-7544-41b9-bd5d-579d85e5d5ad'] = true;
		vm.selectedFunctions['a695b002-eda8-42ef-ba19-3388ead3ea2b'] = true;

		var filter = $filter('functionsFilter');
		var filteredArray = filter.unselected(vm.applicationFunctions, vm.selectedFunctions);

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].ChildFunctions.length).toEqual(1);
	});

	it('should show selected parent with unselected children', function() {
		fakeBackend
			.withApplicationFunction({
				FunctionCode: 'GrandParent1',
				FunctionDescription: 'GrandParent1',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'GrandParent1',
				IsSelected: true,
				ChildFunctions: [{
					FunctionCode: 'Parent1',
					FunctionDescription: 'Parent1',
					FunctionId: 'cf07e1ea-204a-4c20-a0cf-48579f09e6a4',
					IsDisabled: false,
					LocalizedFunctionDescription: 'Parent1',
					IsSelected: true,
					ChildFunctions: [{
						FunctionCode: 'Child1',
						FunctionDescription: 'Child1',
						FunctionId: '0d8554a3-e5b9-4d1d-ad11-1b32f9878c31',
						IsDisabled: false,
						LocalizedFunctionDescription: 'Child1',
						IsSelected: true,
						ChildFunctions: [{
							FunctionCode: 'GrandChild1',
							FunctionDescription: 'GrandChild1',
							FunctionId: '942150e9-d0eb-46b7-891e-ff94f41166ea',
							IsDisabled: false,
							LocalizedFunctionDescription: 'GrandChild1',
							IsSelected: false,
							ChildFunctions: []
						}, {
							FunctionCode: 'GrandChild2',
							FunctionDescription: 'GrandChild2',
							FunctionId: '52998389-fac4-42a9-b5d6-82ae4b0c8567',
							IsDisabled: false,
							LocalizedFunctionDescription: 'GrandChild2',
							IsSelected: true,
							ChildFunctions: []
						}]
					}, {
						FunctionCode: 'Child2',
						FunctionDescription: 'Child2',
						FunctionId: 'cd36b81e-6f9e-43da-ad75-e52bb5a65ca8',
						IsDisabled: false,
						LocalizedFunctionDescription: 'Child2',
						IsSelected: false,
						ChildFunctions: []
					}]
				}, {
					FunctionCode: 'Parent2',
					FunctionDescription: 'Parent2',
					FunctionId: 'fdf17d09-e44c-47ab-b053-3afe2cc1a884',
					IsDisabled: false,
					LocalizedFunctionDescription: 'Parent2',
					IsSelected: true,
					ChildFunctions: []
				}]
			});
		$httpBackend.flush();
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		vm.selectedFunctions['cf07e1ea-204a-4c20-a0cf-48579f09e6a4'] = true;
		vm.selectedFunctions['0d8554a3-e5b9-4d1d-ad11-1b32f9878c31'] = true;
		vm.selectedFunctions['52998389-fac4-42a9-b5d6-82ae4b0c8567'] = true;
		vm.selectedFunctions['fdf17d09-e44c-47ab-b053-3afe2cc1a884'] = true;

		var filter = $filter('functionsFilter');
		var filteredArray = filter.unselected(vm.applicationFunctions, vm.selectedFunctions);

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].ChildFunctions.length).toBe(1);
		expect(filteredArray[0].FunctionId).toEqual('f19bb790-b000-4deb-97db-9b5e015b2e8c');
	});

	it('should show selected org data only', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1'
				}]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Team2'
				}]
			}]
		}, []);
		$httpBackend.flush();
		vm.selectedOrgData['40b52c33-fe20-4913-a651-dcf214b88dc3'] = true;
		vm.selectedOrgData['17f20e39-3d42-422c-94ba-305966612cb5'] = true;
		vm.selectedOrgData['c2bbd355-0fcf-402e-858e-ca2bd0f02175'] = true;

		var filter = $filter('dataFilter');
		var filteredOrgData = filter.selected(vm.organizationSelection, vm.selectedOrgData);

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(1)
	});

	it('should not show unselected bu', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1'
				}]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Team2'
				}]
			}]
		}, []);
		$httpBackend.flush();

		var filter = $filter('dataFilter');
		var filteredOrgData = filter.selected(vm.organizationSelection, vm.selectedOrgData);

		expect(filteredOrgData).toEqual(null);
	});

	it('should not show unselected team', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1'
				}, {
					Id: '32dc997f-fd14-4f7b-ba10-ef04762d08c4',
					Name: 'TeamA'
				}]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Team2'
				}]
			}]
		}, []);
		$httpBackend.flush();
		vm.selectedOrgData['40b52c33-fe20-4913-a651-dcf214b88dc3'] = true;
		vm.selectedOrgData['17f20e39-3d42-422c-94ba-305966612cb5'] = true;
		vm.selectedOrgData['32dc997f-fd14-4f7b-ba10-ef04762d08c4'] = true;

		var filter = $filter('dataFilter');
		var filteredOrgData = filter.selected(vm.organizationSelection, vm.selectedOrgData);

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(1);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(1);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes[0].Name).toEqual('TeamA');
	});


	it('should show unselected org data only', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1'
				}]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Team2'
				}]
			}]
		}, []);
		$httpBackend.flush();
		vm.selectedOrgData['40b52c33-fe20-4913-a651-dcf214b88dc3'] = true;
		vm.selectedOrgData['17f20e39-3d42-422c-94ba-305966612cb5'] = true;
		vm.selectedOrgData['c2bbd355-0fcf-402e-858e-ca2bd0f02175'] = true;

		var filter = $filter('dataFilter');
		var filteredOrgData = filter.unselected(vm.organizationSelection, vm.selectedOrgData);

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(1);
	});

	it('should show everything in an unselected BU', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1'
				}]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Team2'
				}]
			}]
		}, []);
		$httpBackend.flush();

		var filter = $filter('dataFilter');
		var filteredOrgData = filter.unselected(vm.organizationSelection, vm.selectedOrgData);

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(2)
	});

	it('should find function for matching searh string', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All'
			}).withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM'
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'Open');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
	}));

	it('should find function for matching searh string not case sensitive', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All'
			}).withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM'
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'open');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
	}));

	it('should find child function for matching searh string', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All'
			}).withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				ChildFunctions: [{
					FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
					LocalizedFunctionDescription: 'Child1',
					ChildFunctions: []
				}]
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'child');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
		expect(filteredArray[0].ChildFunctions.length).toEqual(1);
		expect(filteredArray[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Child1');
	}));

	it('should find child function and parent', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All'
			})
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				ChildFunctions: [{
					FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
					LocalizedFunctionDescription: 'FirstChild',
					ChildFunctions: [{
						FunctionId: '88bd5120-c698-4ccf-8833-884389ab3efe',
						LocalizedFunctionDescription: 'Somethingelse',
						ChildFunctions: []
					}, {
						FunctionId: '24817ca2-8d5d-44de-9edb-bb464e8b2bbf',
						LocalizedFunctionDescription: 'Child1-a',
						ChildFunctions: []
					}]
				}, {
					FunctionId: 'e1214feb-9d8a-47a9-8563-6ef5cec1c3fb',
					LocalizedFunctionDescription: 'SomethingElse2',
					ChildFunctions: []
				}]
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'child');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
		expect(filteredArray[0].ChildFunctions.length).toEqual(1);
		expect(filteredArray[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('FirstChild');
		expect(filteredArray[0].ChildFunctions[0].ChildFunctions.length).toEqual(2);
	}));

	it('should show everything under found parent', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				ChildFunctions: [{
					FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
					LocalizedFunctionDescription: 'Child1',
					ChildFunctions: []
				}, {
					FunctionId: 'e1214feb-9d8a-47a9-8563-6ef5cec1c3fb',
					LocalizedFunctionDescription: 'Child2',
					ChildFunctions: []
				}, {
					FunctionId: '3b3de8a7-31c4-4d19-a0f7-91f397974a0f',
					LocalizedFunctionDescription: 'Child3',
					ChildFunctions: []
				}]
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'Open');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
		expect(filteredArray[0].ChildFunctions.length).toEqual(3);
		expect(filteredArray[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Child1');
		expect(filteredArray[0].ChildFunctions[1].LocalizedFunctionDescription).toEqual('Child2');
		expect(filteredArray[0].ChildFunctions[2].LocalizedFunctionDescription).toEqual('Child3');

	}));

	it('should show everything under found parent HARDER', inject(function($filter) {
		fakeBackend
			.withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'All'
			})
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				ChildFunctions: [{
					FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
					LocalizedFunctionDescription: 'Child1',
					ChildFunctions: [{
						FunctionId: '88bd5120-c698-4ccf-8833-884389ab3efe',
						LocalizedFunctionDescription: 'Somethingelse',
						ChildFunctions: []
					}, {
						FunctionId: '24817ca2-8d5d-44de-9edb-bb464e8b2bbf',
						LocalizedFunctionDescription: 'Child1-a',
						ChildFunctions: []
					}]
				}, {
					FunctionId: 'e1214feb-9d8a-47a9-8563-6ef5cec1c3fb',
					LocalizedFunctionDescription: 'SomethingElse2',
					ChildFunctions: [{
						FunctionId: 'ac496ff5-a448-493b-91b8-ac095053f73e',
						LocalizedFunctionDescription: 'ImAlsoAChild',
						ChildFunctions: []
					}]
				}]
			});
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredArray = filter.filterFunctions(vm.applicationFunctions, 'child');

		expect(filteredArray.length).toEqual(1);
		expect(filteredArray[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
		expect(filteredArray[0].ChildFunctions.length).toEqual(2);
		expect(filteredArray[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Child1');
		expect(filteredArray[0].ChildFunctions[1].LocalizedFunctionDescription).toEqual('SomethingElse2');
		expect(filteredArray[0].ChildFunctions[0].ChildFunctions.length).toEqual(2);
		expect(filteredArray[0].ChildFunctions[1].ChildFunctions.length).toEqual(1);
		expect(filteredArray[0].ChildFunctions[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Child1-a');
		expect(filteredArray[0].ChildFunctions[0].ChildFunctions[1].LocalizedFunctionDescription).toEqual('Somethingelse');
		expect(filteredArray[0].ChildFunctions[1].ChildFunctions[0].LocalizedFunctionDescription).toEqual('ImAlsoAChild');
	}));

	it('should find org data with search string', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			Type: 'BusinessUnit',
			IsSelected: false,
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				IsSelected: false,
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1',
					IsSelected: false
				},
				{
					Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
					Name: 'Somethingelse',
					IsSelected: false
				}
			]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				IsSelected: false,
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Child',
					IsSelected: false
				}]
			}]
		}, []);
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredOrgData = filter.filterOrgData(vm.organizationSelection, 'team');

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(1);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(1);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes[0].Name).toEqual('Team1');
	});

	it('should show all children under found parent', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			Type: 'BusinessUnit',
			IsSelected: false,
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				IsSelected: false,
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1',
					IsSelected: false
				},
				{
					Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
					Name: 'Somethingelse',
					IsSelected: false
				}
			]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				IsSelected: false,
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Child',
					IsSelected: false
				}]
			}]
		}, []);
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredOrgData = filter.filterOrgData(vm.organizationSelection, 'site1');

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(1);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(2);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes[0].Name).toEqual('Somethingelse');
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes[1].Name).toEqual('Team1');
	});

	it('should show all children under found SUPER parent', function() {
		fakeBackend.withOrganizationSelection({
			Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
			Name: 'BU',
			Type: 'BusinessUnit',
			IsSelected: false,
			ChildNodes: [{
				Id: '17f20e39-3d42-422c-94ba-305966612cb5',
				Name: 'Site1',
				IsSelected: false,
				ChildNodes: [{
					Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
					Name: 'Team1',
					IsSelected: false
				},
				{
					Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
					Name: 'Somethingelse',
					IsSelected: false
				}
			]
			}, {
				Id: '586591fc-1708-45fa-a967-6d7f71162072',
				Name: 'Site2',
				IsSelected: false,
				ChildNodes: [{
					Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
					Name: 'Child',
					IsSelected: false
				}]
			}]
		}, []);
		$httpBackend.flush();

		var filter = $filter('newDescriptionFilter');
		var filteredOrgData = filter.filterOrgData(vm.organizationSelection, 'BU');

		expect(filteredOrgData.BusinessUnit.ChildNodes.length).toEqual(2);
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
		expect(filteredOrgData.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(2);
		expect(filteredOrgData.BusinessUnit.ChildNodes[1].ChildNodes.length).toEqual(1);
	});

})
